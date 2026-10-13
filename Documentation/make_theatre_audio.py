"""Original rhythmic theatre battle scores, foyer music and restorative chime."""
from pathlib import Path
import numpy as np
import wave, uuid
R=Path(__file__).resolve().parents[1]/'Assets/Backstage/Resources'
RATE=44100
def freq(midi):return 440*2**((midi-69)/12)
def add(buf,start,duration,midi,gain,timbre='bell'):
 n=int(duration*RATE);t=np.arange(n)/RATE;hz=freq(midi)
 if timbre=='bell':
  env=(1-np.exp(-t*170))*np.exp(-t*3.5)
  v=np.sin(2*np.pi*hz*t)+.25*np.sin(2*np.pi*hz*2.01*t)+.07*np.sin(2*np.pi*hz*3*t)
 elif timbre=='cello':
  env=np.minimum(t/.055,1)*np.minimum(np.maximum(duration-t,0)/.24,1)
  phase=2*np.pi*hz*t+.075*np.sin(2*np.pi*4.2*t)
  v=np.sin(phase)+.18*np.sin(2*phase)+.065*np.sin(3*phase)
 else:
  env=np.minimum(t/.28,1)*np.minimum(np.maximum(duration-t,0)/.3,1)
  phase=2*np.pi*hz*t+.06*np.sin(2*np.pi*3.4*t)
  v=np.sin(phase)+.055*np.sin(2*phase)
 index=int(start*RATE);tail=min(n,len(buf)-index)
 if tail>0:buf[index:index+tail]+=v[:tail]*env[:tail]*gain

def write(name,buf,peak):
 maximum=np.max(np.abs(buf));buf=buf*(peak/max(maximum,1e-6))
 data=(np.clip(buf,-1,1)*32767).astype('<i2')
 p=R/(name+'.wav')
 with wave.open(str(p),'wb') as f:f.setnchannels(1);f.setsampwidth(2);f.setframerate(RATE);f.writeframes(data.tobytes())
 meta=Path(str(p)+'.meta')
 if not meta.exists():
  meta.write_text('fileFormatVersion: 2\nguid: '+uuid.uuid4().hex+'\nAudioImporter:\n  externalObjects: {}\n  serializedVersion: 7\n  defaultSettings:\n    serializedVersion: 2\n    loadType: 0\n    sampleRateSetting: 0\n    sampleRateOverride: 44100\n    compressionFormat: 1\n    quality: 0.8\n    conversionMode: 0\n  platformSettingOverrides: {}\n  forceToMono: 1\n  normalize: 0\n  preloadAudioData: 1\n  loadInBackground: 0\n  ambisonic: 0\n  3D: 0\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n')
 print(name,round(len(buf)/RATE,2),'seconds','peak',round(np.max(np.abs(buf)),3),'RMS',round(np.sqrt(np.mean(buf*buf)),3))

# Rhythmic original theatre scores: pizzicato, ticking wood and restrained drums.
rng=np.random.default_rng(713)
def drum(buf,at,kind,gain):
 duration=.19 if kind=='kick' else .10;n=int(duration*RATE);t=np.arange(n)/RATE
 if kind=='kick':v=np.sin(2*np.pi*(62*t+42*(1-np.exp(-t*25))/25))*np.exp(-t*24)
 else:
  noise=rng.uniform(-1,1,n);noise=np.concatenate(([0],np.diff(noise)))*.38
  v=(noise if kind=='hat' else noise*.7+np.sin(2*np.pi*185*t)*.3)*np.exp(-t*(85 if kind=='hat' else 42))
 index=round(at*RATE);tail=min(n,len(buf)-index)
 if tail>0:buf[index:index+tail]+=v[:tail]*gain

def battle(name,bpm,bars,boss):
 beat=60/bpm;bar=beat*4;music=np.zeros(round(bar*bars*RATE))
 progression=[36,32,34,31];pattern=[0,7,12,7,3,7,12,15]
 for i in range(bars):
  root=progression[i%4];start=i*bar
  for j in range(8):
   note=root+pattern[j]+(12 if boss else 24)
   add(music,start+j*beat/2,beat*.38,note,.085 if boss else .047,'cello')
   if boss and j%2==0:add(music,start+j*beat/2,beat*.48,root+24+(0,3,7,10)[(i+j//2)%4],.048,'bell')
   drum(music,start+j*beat/2,'hat',.021 if boss else .009)
  for j in range(4):
   add(music,start+j*beat,beat*.6,root+(12 if j%2 else 0),.12 if boss else .055,'cello')
   if j%2==0:drum(music,start+j*beat,'kick',.12 if boss else .035)
   else:drum(music,start+j*beat,'snare',.066 if boss else .016)
  if boss and i%4==3:
   for j in range(4):drum(music,start+3*beat+j*beat/4,'snare',.025+j*.008)
 for delay,gain in ((beat/2,.11),(beat,.045)):music+=np.roll(music,round(delay*RATE))*gain
 fade=round(.015*RATE);music[:fade]*=np.linspace(0,1,fade);music[-fade:]*=np.linspace(1,0,fade)
 write(name,music,.68 if boss else .60)
battle('BossTheme',144,16,True)
battle('CombatTheme',108,12,False)
heal=np.zeros(int(1.05*RATE))
for i,note in enumerate((72,76,79,84)):add(heal,i*.075,.75,note,.28,'bell')
heal+=np.roll(heal,int(.085*RATE))*.14;heal[-int(.1*RATE):]*=np.linspace(1,0,int(.1*RATE))
write('HealChime',heal,.72)
# A sparse, quieter companion theme for the foyer and narrated scenes.
beat=.75;bar=beat*3
foyer=np.zeros(int(bar*12*RATE))
for i,root in enumerate((48,48,44,46,48,44,41,43,48,46,44,43)):
 start=i*bar;third=4 if root==43 else 3
 for note in (root,root+third,root+7):add(foyer,start,bar,note,.026,'pad')
 add(foyer,start+.15,1.8,root+12,.060,'bell')
 add(foyer,start+beat*1.6,1.2,root+19,.039,'bell')
for delay,gain in ((.23,.20),(.41,.10),(.63,.05)):foyer+=np.roll(foyer,int(delay*RATE))*gain
fade=int(.045*RATE);foyer[:fade]*=np.linspace(0,1,fade);foyer[-fade:]*=np.linspace(1,0,fade)
write('MenuAmbience',foyer,.66)
