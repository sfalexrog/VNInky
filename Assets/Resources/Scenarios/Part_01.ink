CONST genderBoy = 0
CONST genderGirl = 1

-> start

EXTERNAL gender()

=== function gender() ===
    ~ return genderGirl


=== start ===

Location: {gender() == genderBoy : Комната М | Комната Д}
{gender() == genderBoy: Ты далеко не любитель рано просыпаться | Ты далеко не любительница рано просыпаться}
А просыпаться 1 сентября - тем более.
Первые десять минут ты обычно просто лежишь и тупишь в смартфон.
Это помогает проснуться.
В это утро остатки сна покинули тебя раньше обычного,
Потому что тебе пришло странное сообщение:
«Смотри скорее, там такое…»
Я(Удивление): О, это еще что? # удивление
Я(Удивление): Какие-то новости из старой школы? # удивление
Я: Больше похоже на какой-то прикол. 
Но любопытство взяло верх...
Тебя перекинуло на страницу ВК.
Я: “Введите логин-пароль”...
Я(Злость): Зашибись!
Я: Еще вчера все работало!
Я: Окей, вот тебе мой логин-пароль.
...: Но стоило ввести логин-пароль и нажать “ок”...
...: Как смартфон загадочно подмигнул тебе и перезагрузился.
Я(Злость): Класс! Батарея села!
Я: Самое время.
...: Тук-тук!
Мама(Радость): Проснулось, Красно-солнышко? 
Мама: Бегом одеваться! Завтрак стынет!

Локация: Кухня.
...: На кухне солнечно и приятно пахнет блинами. 
...: Ты хватаешь еще горячий блин и макаешь его в варенье.
Мама(Радость): Ну, как блины?
Я(Радость): Отлично, мам. 
...: Мама всегда поддерживает тебя.
...: Сегодня она специально встала пораньше, чтобы приготовить твои любимые блины.
...: Она очень хочет быть твоим другом.
...: Но для тебя она все таки больше мама.
Мама: Волнуешься в свой первый день в школе?
Я(Выбор): Ну...
*[Нет, с чего бы?]
->Conflict01
*[ Да, я боюсь облажаться…]
->Conflict01
== Conflict01 ==
Мама(Радость): Ой, брось. Волноваться - это нормально. 
Мама(Радость): Я три раза меняла школу.
Мама: Только запомни:
Мама: Если тебя задирают - не бойся дать сдачи,
Мама: И рассказать взрослым о своих проблемах.
Я(Печаль): Спасибо, мам. 
Я(Печаль): Ты умеешь успокоить.
Мама: А как твои друзья? Писали тебе летом?
Я: Угу. Сегодня вот один прислал…
...: Ты вспомнил то странное сообщение![Ты вспомнила то странное сообщение!]
...: Интересно, что там?
Я: Мам, спасибо! Мне пора! 
Мама(Удивление): Так, погоди!
Мама(Злость): Помнишь, о чем мы с тобой договаривались?
Мама:Теперь ты сам моешь тарелку за собой.
Я(Выбор)(Печаль): Ну мам...
*[Только не сегодня…]
->Conflict02
*[Ладно, вымою…]
->Conflict02
== Conflict02 ==
Мама: И не надо на меня так смотреть! 
Мама: Это не я люблю качать права, что “уже не ребенок”.
...: Тебе пришлось вымыть посуду.
...: Наверное, не случилось бы ничего плохого,
...: Оставь ты это до вечера...
...: Но уговор есть уговор.
Мама(Радость): Теперь я вижу, что у меня растет настоящий помощник!

Локация: Комната м[Комната д]
...: Смартфон уже должен был зарядиться.
...: Тебе не терпелось прочесть наконец сообщение.
...: Только твоя рука потянулась за смартфоном, как…
Мама(Удивление): Глазам не верю!
Мама(Удивление): Мы только три недели назад как переехали,
Мама(Удивление): а у тебя опять на полу носки и бумажки разбросаны!
Я: Я все уберу вечером!
Мама(Злость): Нет, давай уж уберись сейчас. 
Мама: Кто из нас “уже не ребенок”?
Я(Выбор): Мам...
*[Сейчас мигом тут все уберу!]
Мама(Радость): Умничка!
Мама: А теперь бегом в школу!
->Conflict03
*[Я опаздываю в школу!]
Мама(Злость): Знаю я твои “опаздываю”.
Мама: Что ж, беги тогда.
->Conflict03
== Conflict03 ==
Мама: И посмотри, не пришли ли квитанции за газ!

Локация: Подъезд дома
Я: Ну наконец-то я прочитаю что там!
Я(Удивление): Что за фигня?
Я: Да как так “пароль неверный”?!
Я(Злость): Не могли же у меня за полчаса угнать пароль!
...: Раз за разом выходило одно и то же: “Неверный логин или пароль”!
...: Тебе еще и пришлось копаться в почтовом ящике
...: Похоже, почтальон путает его с мусорной корзиной.
...: Столько бумажек!
...: На пол выпала яркая красно-зеленая листовка.
Я(Удивление): Хм… А это еще что?
Я: «Лаборатория Компьютерной Безопасности. У вас проблемы? Мы их решим!»
->DONE
->END


