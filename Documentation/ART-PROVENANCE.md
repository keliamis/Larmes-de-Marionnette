# Иллюстрации и звук — редакция 3.12

Все восемь активных иллюстраций созданы встроенным ImageGen по одному мастер-кадру меню, основанному на пользовательском референсе. Они заменяют меню, загрузку, карту актов, четыре кадра предыстории и финал. Резервные сюжетные панели и экран дуэли используют эту же серию.

Мастер-стиль: плоская атмосферная 2D иллюстрация кукольного театра, крупные цветовые формы, два тона на поверхность, мягкая бумажная фактура, тёмно-синий фон, бордовые занавески, янтарный свет. Мрачная сказочная атмосфера Little Nightmares адаптирована к иллюстрации с небольшой глубиной, без реалистичного 3D объёма. Кукла сохраняет бордовый колпак, светлые волосы, голубое платье и деревянные суставы; у неё две руки и две ноги. Тень — простой чёрный гуманоид с двумя светящимися глазами.

## Prompt set

Shared prompt: “Create one wide 16:9 fullscreen image for the same game in exactly the attached master image’s flat illustrated treatment, character designs and palette. Hand-painted 2D theater illustration, broad simple shapes, two tones per surface, subtle paper texture, three background layers, restrained details, low volume. Dark fairy-tale atmosphere influenced by Little Nightmares, rendered as 2D. Navy shadows, burgundy curtains, amber light. Same girl: burgundy pointed cap with gold tip, blond hair, pale blue apron dress, wooden joints, burgundy shoes. Same shadow: smooth black humanoid, round head, two ivory glowing eyes, no crown or flames. No text, no UI, no 3D render. Entire figures visible, girl has two arms and two legs.”

Scenes: menu — seated pair at center-right with free dark space for menu and title; loading — running pair before paper moon; act map — three connected miniature theater stages; lonely — girl suspended by strings; awakening — girl meets independent shadow; confrontation — shadow shields girl from mechanical puppeteer hand; revolt — pair crosses theater walkway toward light; finale — freed pair faces dawn while mechanical hand and broken strings recede.

Рука Кукловода в иллюстрациях имеет бордовую манжету, латунную ладонь, суставные пальцы и красный камень. В финальной игровой анимации и в бою используется одна функция HandModel, то есть одна геометрия.

PNG-оригиналы сохранены без потерь как Resources/*.bytes. Текст и интерактивные кнопки рисуются отдельно. Фигурные деревянные таблички созданы скриптом make_theatre_buttons.py. В редакции 3.6 название заменено иллюстрационным леттерингом ImageGen: две строки «Larmes de / Marionnette», золотые завитки и театральный орнамент. Живой UI использует системный шрифт.

В редакции 3.5 обновлены деревянные таблички с резными краями, игровые глаза зайцев и декорация-щит. Два WAV-ресурса построены из синтезированных тонов без внешних семплов: BossTheme — 24-секундная минорная тема, HealChime — короткий восходящий звон. Служебный генератор: make_theatre_audio.py.

В редакции 3.6 встроенным ImageGen созданы MenuStage (фон без персонажей и надписей), MenuCharacters (прозрачный слой сидящей девочки и тени) и TitleWordmark (прозрачное рисованное название). Для композиции использован последний пользовательский референс меню и прежний мастер-кадр для палитры и дизайна. Отдельной правкой ImageGen исправлен контур головы тени: гладкий округлый силуэт без лишних отростков. Бордовый колпак, платье и деревянные суставы девочки сохранены. Прозрачные поля обрезаются только при отрисовке по альфа-границе, исходные PNG не изменены.

MenuAmbience — оригинальная 27-секундная синтезированная тема шкатулки с мягким аккомпанементом; внешние записи и семплы не используются. Она создана тем же make_theatre_audio.py. Свечение, частицы, цепочка табличек и параллакс рисуются кодом, без новых кадров или текстур на каждом обновлении.

В 3.7 активная текстура PaperButton/PaperButtonHover заменена встроенным ImageGen по пользовательскому референсу image(20261012-190314).png: одна широкая подвешенная табличка из старой бумаги, рваные края, латунные крепления, свободный центр без текста, прозрачный фон. Один оригинал используется для обоих состояний; наведение меняет цвет и масштаб кодом. Генератор make_theatre_buttons.py относится к прежней редакции и не воспроизводит новую текстуру.

LoadingPremium сохранён как иллюстрация заставки: бегущая девочка в колпаке и тень перед большой луной. По последнему референсу размещены полоса перехода и подпись; добавлены плавное перекрытие экрана и автоматическое движение меню.

В 3.8 встроенным ImageGen созданы PaperSigns (единый прозрачный атлас шести разных бумажных знаков: рваный, с загнутыми краями, на деревянной основе, с фигурными концами, с подвесками и с потёртой поверхностью) и DuelIllustration (две тени на театральной сцене). Источники стиля — последний пользовательский референс и актуальное главное меню. PNG сохранены без потерь в Resources; области атласа заданы по прозрачным промежуткам, без редактирования оригинала. Отдельные старые PaperButton/PaperButtonHover удалены.

Prompt set 3.8: six separate blank antique paper signs on transparent background, distinct silhouettes/shades/fittings, flat hand-painted theatre palette, no lettering; inset illustration of two smooth round-headed shadow duelists, two arms and legs each, ivory eyes, burgundy curtains, hanging gold stars and crescent moon, navy shadows and amber lighting, matching main menu, no text or UI.

В 3.9 оригинальные синтезированные боевые темы обновлены генератором make_theatre_audio.py: CombatTheme — 108 BPM, 12 тактов 4/4; BossTheme — 144 BPM, 16 тактов 4/4. Обе длительностью 26,67 секунды. Использованы синтезированные струнные тоны, колокольчики, бас-бочка и шумовые ударные с фиксированным seed; внешних записей нет. Меню и лечение сохранили свои мелодии, лечение сведено тише.

В 3.12 встроенным ImageGen заново созданы пять сюжетных ресурсов по композиционному предпросмотру текущего главного меню. Исходные PNG перенесены без редактирования в Assets/Backstage/Resources/StoryLonely.bytes, StoryAwakens.bytes, StoryHand.bytes, StoryRevolt.bytes и FinalPremium.bytes; GUID активных ресурсов сохранены. Все резервные сюжетные панели теперь ссылаются на эти же файлы, без отдельных устаревших копий.

Prompt set 3.12:
- Shared: full-screen 16:9 story illustration in exactly the main-menu reference's polished flat hand-painted 2D style; navy darkness, burgundy curtains, amber light, faceted paint and paper texture; same blond wooden girl in pointed burgundy cap, blue apron dress, cream blouse and burgundy shoes; smooth round-headed black shadow with ivory eyes; exactly two continuous arms and two legs per figure, no ghost limbs, no text/UI. Dark uncluttered upper-left for story text.
- Lonely: girl suspended above wooden stage, both arms down, four strings attached to actual wrist and shoulder joints, no detached hand.
- Awakens: seated girl and shadow discover one another, hands on floor or knees, feet visible, no overlapping clasp.
- Hand: girl stands behind shadow with hands clearly at her sides; shadow raises one arm toward brass puppeteer hand and extends the other to protect her; burgundy cuff and red jewel.
- Revolt: shadow ahead on right walks to exit, reaching its left arm back to hold girl's right hand; girl behind on left follows, one joined pair of hands only.
- Final: freed pair sits on stage facing dawn, two arms and legs each, shadow unarmed; mechanical hand retreats in upper-left with broken detached strings; calm upper-right space for ending text.

Кнопочные звуки 3.12 синтезируются существующим PlayTone: hover 560 Hz / 45 ms / gain 0,012, click 310 Hz / 75 ms / gain 0,023. Внешних аудиозаписей нет.
