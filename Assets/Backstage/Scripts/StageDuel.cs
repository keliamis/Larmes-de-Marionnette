using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace Backstage
{
    // Small, self-contained LAN mode. All gameplay is simulated on the host.
    // Protocol: HELLO=0, INPUT=1, SNAPSHOT=2. No internet server is needed.
    // Intended for a trusted home / classroom network, not public matchmaking.
    public sealed class DuelShot
    {
        public Vector2 Position,Velocity;
        public bool HostShot;
        public int Bounces;
        public float Life=3.6f;
    }
    public sealed class StageDuel : IDisposable
    {
        public const int Port=39393;
        public static readonly Barrier[] Cover={
            new Barrier{A=new Vector2(-2.7f,-3.3f),B=new Vector2(-1.1f,.2f),Width=.7f},
            new Barrier{A=new Vector2(2.7f,3.3f),B=new Vector2(1.1f,-.2f),Width=.7f}
        };
        public bool Active=true,Connected,IsHost,Finished;
        public string Status="Соединяемся…";
        public int HostWins,GuestWins,Round=1,HostHP=5,GuestHP=5;
        public Vector2 HostPosition=new Vector2(-8,0),GuestPosition=new Vector2(8,0);
        public Vector2 HostAim=Vector2.right,GuestAim=Vector2.left;
        public float HostFlash,GuestFlash,RoundPause;
        public readonly List<DuelShot> Shots=new List<DuelShot>();
        readonly UdpClient udp;
        IPEndPoint peer;
        ShotInput guestInput;
        bool hostDashQueued,guestDashQueued;
        float clock,sent,lastContact,simulation,hostReload,guestReload,hostDash,guestDash;
        float hostInvulnerable,guestInvulnerable;
        public bool Ready { get {return Connected&&!Finished&&RoundPause<=0;} }
        StageDuel(UdpClient c,bool host){udp=c;udp.Client.Blocking=false;IsHost=host;Status=host?"Ждём второго игрока. Порт 39393":"Поиск хоста…";}
        public static StageDuel Host()
        {
            UdpClient socket=new UdpClient(AddressFamily.InterNetwork);
            try{socket.Client.Bind(new IPEndPoint(IPAddress.Any,Port));return new StageDuel(socket,true);}
            catch{socket.Close();throw;}
        }
        public static StageDuel Join(string address)
        {
            IPAddress ip;
            if(!IPAddress.TryParse(address.Trim(),out ip) || ip.AddressFamily!=AddressFamily.InterNetwork)
                throw new ArgumentException("Укажи IPv4 хоста, например 192.168.1.15");
            UdpClient socket=new UdpClient(AddressFamily.InterNetwork);
            try{socket.Client.Bind(new IPEndPoint(IPAddress.Any,0));var d=new StageDuel(socket,false);d.peer=new IPEndPoint(ip,Port);return d;}
            catch{socket.Close();throw;}
        }
        public void Dispose(){Active=false;udp.Close();}
        void Send(byte[] data)
        {
            if(peer==null)return;
            try {udp.Send(data,data.Length,peer);}catch(SocketException ex){Status="Сеть: "+ex.SocketErrorCode;}
            catch(ObjectDisposedException){Active=false;}
        }
        void Receive()
        {
            int cap=0;
            while(Active&&udp.Available>0&&cap++<32)
            {
                try
                {
                    IPEndPoint sender=new IPEndPoint(IPAddress.Any,0);
                    byte[] data=udp.Receive(ref sender);
                    if(data.Length<1||data.Length>4096)continue;
                    if(IsHost)
                    {
                        if(data[0]==0 && (peer==null||peer.Equals(sender)))
                        {peer=sender;Connected=true;Status="Игрок подключён";lastContact=clock;SendState();}
                        else if(Connected&&peer.Equals(sender)&&data[0]==1&&data.Length>=19)
                        {
                            using(var stream=new MemoryStream(data))using(var reader=new BinaryReader(stream))
                            {
                                reader.ReadByte();
                                float mx=reader.ReadSingle(),my=reader.ReadSingle(),ax=reader.ReadSingle(),ay=reader.ReadSingle();
                                guestInput=new ShotInput{Move=Safe(new Vector2(mx,my)),Aim=Safe(new Vector2(ax,ay)),Fire=reader.ReadBoolean(),Dash=reader.ReadBoolean()};
                            }
                            lastContact=clock;
                        }
                    }
                    else if(peer!=null&&peer.Equals(sender)&&data[0]==2)
                    {
                        using(var stream=new MemoryStream(data))using(var reader=new BinaryReader(stream))
                        {
                            reader.ReadByte();
                            int newRound=reader.ReadByte();
                            int hw=reader.ReadByte(),gw=reader.ReadByte();
                            int hh=reader.ReadByte(),gh=reader.ReadByte();
                            bool done=reader.ReadBoolean();float pause=reader.ReadSingle();
                            Vector2 hp=ReadVector(reader),gp=ReadVector(reader),ha=ReadVector(reader),ga=ReadVector(reader);
                            int count=reader.ReadByte();
                            if(count>90)continue;
                            Shots.Clear();
                            for(int i=0;i<count;i++)
                            {
                                Vector2 pos=ReadVector(reader);bool hostShot=reader.ReadBoolean();
                                Shots.Add(new DuelShot{Position=pos,HostShot=hostShot});
                            }
                            if(!Finite(hp)||!Finite(gp))continue;
                            Connected=true;Status="Дуэль началась";lastContact=clock;
                            Round=newRound;HostWins=hw;GuestWins=gw;HostHP=hh;GuestHP=gh;Finished=done;RoundPause=pause;
                            HostPosition=hp;GuestPosition=gp;HostAim=ha;GuestAim=ga;
                        }
                    }
                }
                catch(SocketException){break;}
                catch(EndOfStreamException){/* Drop truncated datagram. */}
                catch(IOException){/* Drop invalid datagram. */}
            }
        }
        static bool Finite(Vector2 p){return !float.IsNaN(p.x)&&!float.IsNaN(p.y)&&!float.IsInfinity(p.x)&&!float.IsInfinity(p.y);}
        static Vector2 Safe(Vector2 p){return Finite(p)?Vector2.ClampMagnitude(p,1):Vector2.zero;}
        static Vector2 ReadVector(BinaryReader r){return new Vector2(r.ReadSingle(),r.ReadSingle());}
        static void WriteVector(BinaryWriter w,Vector2 p){w.Write(p.x);w.Write(p.y);}
        void SendState()
        {
            if(!Connected)return;
            using(var stream=new MemoryStream(2048))using(var writer=new BinaryWriter(stream))
            {
                writer.Write((byte)2);writer.Write((byte)Mathf.Clamp(Round,0,255));
                writer.Write((byte)HostWins);writer.Write((byte)GuestWins);
                writer.Write((byte)HostHP);writer.Write((byte)GuestHP);
                writer.Write(Finished);writer.Write(RoundPause);
                WriteVector(writer,HostPosition);WriteVector(writer,GuestPosition);WriteVector(writer,HostAim);WriteVector(writer,GuestAim);
                int count=Mathf.Min(Shots.Count,70);writer.Write((byte)count);
                for(int i=0;i<count;i++){WriteVector(writer,Shots[i].Position);writer.Write(Shots[i].HostShot);}
                Send(stream.ToArray());
            }
        }
        void SendInput(ShotInput input)
        {
            using(var stream=new MemoryStream(24))using(var writer=new BinaryWriter(stream))
            {
                writer.Write((byte)1);WriteVector(writer,Safe(input.Move));WriteVector(writer,Safe(input.Aim));writer.Write(input.Fire);writer.Write(input.Dash);
                Send(stream.ToArray());
            }
        }
        void NewRound()
        {
            Shots.Clear();HostHP=GuestHP=5;HostPosition=new Vector2(-8,0);GuestPosition=new Vector2(8,0);
            HostAim=Vector2.right;GuestAim=Vector2.left;hostInvulnerable=guestInvulnerable=1.1f;
            hostReload=guestReload=hostDash=guestDash=0;RoundPause=0;
        }
        public void Tick(float dt,ShotInput local)
        {
            if(!Active)return;
            dt=Mathf.Clamp(dt,0,.05f);clock+=dt;
            if(local.Dash){if(IsHost)hostDashQueued=true;else guestDashQueued=true;}
            Receive();
            if(!Connected)
            {
                if(!IsHost&&clock-sent>.5f){Send(new byte[]{0});sent=clock;}
                return;
            }
            if(clock-lastContact>7f)
            {Connected=false;Status="Связь потеряна. Вернись в меню и повтори подключение.";return;}
            if(!IsHost)
            {
                if(clock-sent>.045f){local.Dash=guestDashQueued;SendInput(local);guestDashQueued=false;sent=clock;}
                return;
            }
            simulation+=dt;
            while(simulation>=1f/60)
            {
                local.Dash=hostDashQueued;Simulate(1f/60,local,guestInput);hostDashQueued=false;
                simulation-=1f/60;
            }
            if(clock-sent>=.05f){SendState();sent=clock;}
        }
        void Simulate(float dt,ShotInput h,ShotInput g)
        {
            HostFlash=Mathf.Max(0,HostFlash-dt);GuestFlash=Mathf.Max(0,GuestFlash-dt);
            hostInvulnerable=Mathf.Max(0,hostInvulnerable-dt);guestInvulnerable=Mathf.Max(0,guestInvulnerable-dt);
            if(Finished)return;
            if(RoundPause>0){RoundPause-=dt;if(RoundPause<=0){Round++;NewRound();}return;}
            hostReload-=dt;guestReload-=dt;hostDash-=dt;guestDash-=dt;
            h.Move=Safe(h.Move);g.Move=Safe(g.Move);
            if(h.Aim.sqrMagnitude>.01f)HostAim=Safe(h.Aim).normalized;
            if(g.Aim.sqrMagnitude>.01f)GuestAim=Safe(g.Aim).normalized;
            bool hd=h.Dash&&hostDash<=0&&h.Move.sqrMagnitude>.01f;
            bool gd=g.Dash&&guestDash<=0&&g.Move.sqrMagnitude>.01f;
            if(hd)hostDash=2; if(gd)guestDash=2;
            HostPosition=MoveDuel(HostPosition,h.Move*(hd?12:5.1f)*dt);
            GuestPosition=MoveDuel(GuestPosition,g.Move*(gd?12:5.1f)*dt);
            if(h.Fire&&hostReload<=0){Fire(HostPosition,HostAim,true);hostReload=.42f;}
            if(g.Fire&&guestReload<=0){Fire(GuestPosition,GuestAim,false);guestReload=.42f;}
            for(int i=Shots.Count-1;i>=0;i--)
            {
                DuelShot b=Shots[i];b.Life-=dt;
                if(b.Life<=0){Shots.RemoveAt(i);continue;}
                Vector2 delta=b.Velocity*dt,n;float t;
                if(TraceCover(b.Position,delta,.12f,out t,out n))
                {
                    b.Position+=delta*Mathf.Clamp01(t);
                    if(b.Bounces>=2){Shots.RemoveAt(i);continue;}
                    b.Velocity-=2*Vector2.Dot(b.Velocity,n)*n;b.Position+=n*.004f;b.Bounces++;
                }
                else b.Position+=delta;
                Vector2 target=b.HostShot?GuestPosition:HostPosition;
                if((b.Position-target).sqrMagnitude<.55f*.55f)
                {
                    if(b.HostShot && guestInvulnerable<=0){GuestHP--;guestInvulnerable=.7f;GuestFlash=.28f;}
                    if(!b.HostShot&&hostInvulnerable<=0){HostHP--;hostInvulnerable=.7f;HostFlash=.28f;}
                    Shots.RemoveAt(i);
                }
            }
            if(GuestHP<=0||HostHP<=0)
            {
                if(GuestHP<=0)HostWins++;else GuestWins++;
                Finished=HostWins>=3||GuestWins>=3;RoundPause=Finished?999:2.6f;Shots.Clear();
            }
        }
        static bool TraceCover(Vector2 p,Vector2 delta,float r,out float fraction,out Vector2 normal)
        {
            StageCollision.Border(p,delta,r,out fraction,out normal);
            foreach(Barrier b in Cover)
            {
                float f;Vector2 n;
                if(StageCollision.Capsule(p,delta,b.A,b.B,b.Width*.5f+r,out f,out n)&&f<fraction)
                {fraction=f;normal=n;}
            }
            return fraction<=1;
        }
        static Vector2 MoveDuel(Vector2 p,Vector2 motion)
        {
            int steps=Mathf.Max(1,Mathf.CeilToInt(motion.magnitude/.12f));
            for(int i=0;i<steps;i++)
            {
                p=StageWorld.Clamp(p+motion/steps,.55f);
                foreach(Barrier b in Cover)
                {
                    Vector2 ab=b.B-b.A;
                    float t=Mathf.Clamp01(Vector2.Dot(p-b.A,ab)/Mathf.Max(.001f,ab.sqrMagnitude));
                    Vector2 near=b.A+ab*t,delta=p-near;
                    float clear=b.Width*.5f+.55f;
                    if(delta.sqrMagnitude<clear*clear)
                    {
                        if(delta.sqrMagnitude<.001f)delta=new Vector2(-ab.y,ab.x).normalized;
                        p=StageWorld.Clamp(near+delta.normalized*clear,.55f);
                    }
                }
            }
            return p;
        }
        void Fire(Vector2 p,Vector2 dir,bool host)
        {
            if(Shots.Count>=70)return;
            Shots.Add(new DuelShot{Position=p+dir*.62f,Velocity=dir*12.2f,HostShot=host});
        }
    }
}
