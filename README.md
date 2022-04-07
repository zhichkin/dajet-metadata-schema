# DaJet JSON Schema

**1. Примитивные типы данных.**
- [Неопределенно](#Неопределенно)
- [Булево](#Булево)
- [Число](#Число)
- [Строка](#Строка)
- [Дата](#Дата)
- [УникальныйИдентификатор](#УникальныйИдентификатор)
- [Бинарные данные](#Бинарные-данные)

**2. Ссылочные типы данных.**
- [Ссылка](#Ссылка)
- [Перечисление](#Перечисление)

**3. Составной тип данных.**
- [Составной тип (oneOf)](#Составной-тип)

**4. Объекты ссылочных типов данных.**
- [Объект ссылочного типа (документ или справочник)](#Ссылочный-объект)

**5. Наборы записей регистров.**
- [Набор записей (регистры сведений или накопления)](#Набор-записей)



#### Неопределенно
Полностью соответствует типу данных JSON - null.
```json
{ "ИмяСвойства": null }
```

#### Булево
Полностью соответствует типу данных JSON - boolean.
```json
{ "ИмяСвойства": true }
{ "ИмяСвойства": false }
```

#### Число
Полностью соответствует типу данных JSON - number.
```json
{ "ИмяСвойства": 12345 }
{ "ИмяСвойства": -1234 }
{ "ИмяСвойства": 1.234 }
{ "ИмяСвойства": -1.23 }
```

#### Строка
Полностью соответствует типу данных JSON - string.
```json
{ "ИмяСвойства": "Это строковое значение." }
```

#### Дата
Кодируется как строка в формате ISO-8601: yyyy-MM-dd HH:mm:ss.
```json
{ "ИмяСвойства": "2022-01-15 14:30:00" }
```

#### УникальныйИдентификатор
Кодируется как строковое представление UUID.
```json
{ "ИмяСвойства": "26093579-c180-11e4-a7a9-000d884fd00d" }
{ "ИмяСвойства": "00000000-0000-0000-0000-000000000000" }
```

#### Бинарные данные
Бинарные данные кодируются в формате Base64.
```json
{ "ИмяСвойства": "0K3RgtC+INCx0LjQvdCw0YDQvdGL0LUg0LTQsNC90L3Ri9C1" }
```

#### Ссылка
Ссылка - это указатель на конкретный элемент ссылочного объекта в информационной базе 1С.
Кодируется как тип данных УникальныйИдентификатор. В случае кодирования типа ПустаяСсылка
используется нулевое значение UUID.
```json
{ "ИмяСвойства": "26093579-c180-11e4-a7a9-000d884fd00d" }
{ "ИмяСвойства": "00000000-0000-0000-0000-000000000000" }
```

#### Перечисление
Кодируется как перечисление строковых значений.
```json
{ "ИмяСвойства": "НДС_20" }
```

#### Составной тип
Составной тип данных может иметь разные типы данных в качестве своего значения.

Составной тип соответствует семантике ключевого слова **oneOf**,
используемого [JSON Schema](https://json-schema.org/understanding-json-schema/reference/combining.html#oneof).

Значение составного типа кодируется как объект, имеющий два свойства: type и value.

В случае отсутствия значения - кодируется как тип данных Неопределенно (null).

Значениями свойства "type" могут быть:
- boolean - Булево
- number - Число
- string - Строка
- datetime - Дата
- uuid - УникальныйИдентификатор
- binary - Бинарные данные
- имя ссылочного типа, например, Справочник.Номенклатура
```json
{ "ИмяСвойства": null }

{ "ИмяСвойства": { "type": "boolean", "value": true } }

{
   "ИмяСвойства":
   {
      "type": "Справочник.Номенклатура",
      "value": "26093579-c180-11e4-a7a9-000d884fd00d"
   }
}
```

#### Ссылочный объект
Объект ссылочного типа данных кодируется как объект JSON (object).

Эти объекты могут иметь табличные части, выраженные как массивы записей (array).

Ссылочные объекты имеют тот или иной предопределённый платформой 1С набор свойств (общие реквизиты).
Например, реквизит "Ссылка" присутствует у всех объектов ссылочного типа.
Значением этого свойства является уникальный идентификатор элемента данного типа.
```json
{
   "Ссылка": "26093579-c180-11e4-a7a9-000d884fd00d",
   "Дата": "2022-01-15 14:30:00",
   "Номер": "ЦБ000123",
   "Комментарий": "Пример заказа клиента с табличной частью \"Товары\".",
   "Товары":
   [
      {
         "Номенклатура": "26093579-c180-11e4-a7a9-000d884fd00d",
         "Количество": 10,
         "Цена": 1.2,
         "Сумма": 12
      }
   ]
}
```

#### Набор записей
Набор записей регистра кодируется как объект JSON (object),
имеющий два свойства filter (отбор) и records (записи набора).

Свойство filter кодируется как объект JSON (может иметь значение null).

Свойство records кодируется как массив объектов JSON  (может иметь значение null).
```json
{
   "filter":
   {
      "Период": "2022-01-01 00:00:00",
      "Валюта": "26093579-c180-11e4-a7a9-000d884fd00d"
   },
   "records":
   [
      {
         "Период": "2022-01-01 00:00:00",
         "Валюта": "26093579-c180-11e4-a7a9-000d884fd00d",
         "Курс": 1.23
      }
   ]
}
```