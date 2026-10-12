# Единая иллюстрационная серия 3.4

Все восемь активных иллюстраций созданы встроенным ImageGen по одному мастер-кадру меню, основанному на пользовательском референсе. Они заменяют меню, загрузку, карту актов, четыре кадра предыстории и финал. Резервные сюжетные панели и экран дуэли используют эту же серию.

Мастер-стиль: плоская атмосферная 2D иллюстрация кукольного театра, крупные цветовые формы, два тона на поверхность, мягкая бумажная фактура, тёмно-синий фон, бордовые занавески, янтарный свет. Мрачная сказочная атмосфера Little Nightmares адаптирована к иллюстрации с небольшой глубиной, без реалистичного 3D объёма. Кукла сохраняет бордовый колпак, светлые волосы, голубое платье и деревянные суставы; у неё две руки и две ноги. Тень — простой чёрный гуманоид с двумя светящимися глазами.

## Prompt set

Shared prompt: “Create one wide 16:9 fullscreen image for the same game in exactly the attached master image’s flat illustrated treatment, character designs and palette. Hand-painted 2D theater illustration, broad simple shapes, two tones per surface, subtle paper texture, three background layers, restrained details, low volume. Dark fairy-tale atmosphere influenced by Little Nightmares, rendered as 2D. Navy shadows, burgundy curtains, amber light. Same girl: burgundy pointed cap with gold tip, blond hair, pale blue apron dress, wooden joints, burgundy shoes. Same shadow: smooth black humanoid, round head, two ivory glowing eyes, no crown or flames. No text, no UI, no 3D render. Entire figures visible, girl has two arms and two legs.”

Scenes: menu — seated pair at center-right with free dark space for menu and title; loading — running pair before paper moon; act map — three connected miniature theater stages; lonely — girl suspended by strings; awakening — girl meets independent shadow; confrontation — shadow shields girl from mechanical puppeteer hand; revolt — pair crosses theater walkway toward light; finale — freed pair faces dawn while mechanical hand and broken strings recede.

Рука Кукловода в иллюстрациях имеет бордовую манжету, латунную ладонь, суставные пальцы и красный камень. В финальной игровой анимации и в бою используется одна функция HandModel, то есть одна геометрия.

PNG-оригиналы сохранены без потерь как Resources/*.bytes. Текст и интерактивные кнопки рисуются отдельно. Бумажные полосы и точный текст названия созданы скриптами make_paper_buttons.py и make_wordmark.py. Файлы шрифтов для названия не распространяются: поставляется готовая прозрачная картинка; живой UI использует системный шрифт.
