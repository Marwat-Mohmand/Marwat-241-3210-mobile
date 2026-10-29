# HomeWork3 — prjPasswordCreator

Мохманд Марватулла, 241-3210.

Законченный проект «Генератор паролей» по [заданию](https://docs.google.com/document/d/1-1PFM0usyQT2__cyrIiK5HKUTkArZ16aqhRvQ7hBEm0). Эта работа полностью самостоятельна: каталог HomeWork4 для её сборки не нужен.

## Выполнение требований

| Пункт документа | Реализация |
| --- | --- |
| Доработать библиотеку (lib) | `libPasswordCreator`: неизменяемые настройки, проверка параметров, криптографическая генерация и перемешивание |
| Консольная версия (cns) | `cnsPasswordCreator`: интерактивный ввод, аргументы командной строки, экспорт TXT, коды завершения 0/1 |
| WinForms (wfa) | `wfaPasswordCreator`: отдельное Windows-приложение со всеми базовыми настройками |
| Добавить WPF | `wpfPasswordCreator`: XAML, привязки, проверка числового ввода, общий ViewModel |
| Добавить MAUI | `mauiPasswordCreator`: XAML-интерфейс, Android/Windows, точки входа iOS/Mac Catalyst |
| Улучшить реализацию | Логика без зависимости от UI, общая модель представления, проверки ошибок, автоматические тесты |
| Режимы и настройки | Три готовых профиля, длина и количество, четыре независимые группы символов |

## Возможности и ограничения

- Длина 1–256 символов, количество 1–100; по умолчанию 16 символов и 5 паролей.
- Группы: `a–z`, `A–Z`, `0–9`, `!@#$%^&*()-_=+[]{};:,.?`.
- В каждом пароле есть хотя бы один символ каждой выбранной группы. Длина должна быть не меньше количества выбранных групп.
- Профили: «Надёжный», «Без спецсимволов», «Только цифры» (6 цифр). Цифровой профиль предназначен для PIN, его стойкость ниже длинного случайного пароля.
- `RandomNumberGenerator.GetInt32` для каждого выбора; Fisher–Yates для перемешивания. Генерация включает обязательные символы групп и не обещает равномерного распределения по всем допустимым строкам.
- Копирование всего результата, сохранение TXT, очистка. MAUI экспортирует через системное меню «Поделиться».
- Показана **верхняя оценка** пространства поиска `Length × log2(AlphabetSize)`, а не фактическая энтропия или время взлома. Ограничение обязательных групп уменьшает реальное пространство.
- Повторы символов и совпадение разных паролей допускаются. Пароли не сохраняются автоматически; экспортированный TXT содержит открытый текст.

## Структура

```text
prjPasswordCreator/
  libPasswordCreator/              # генерация и параметры
  PasswordCreator.Presentation/    # INotifyPropertyChanged, команды, профили
  cnsPasswordCreator/              # Console
  wfaPasswordCreator/              # WinForms
  wpfPasswordCreator/              # WPF
  mauiPasswordCreator/             # MAUI + платформенные файлы и ресурсы
  PasswordCreator.Tests/           # исполняемые проверки без NuGet-зависимостей
  prjPasswordCreator.slnx          # полное решение, включая MAUI
  prjPasswordCreator.Desktop.slnx  # Console + WinForms + WPF + тесты
```

## Сборка и запуск

Требуется .NET SDK 10. Для WinForms/WPF нужна Windows. В Visual Studio откройте `.slnx` и назначьте нужный UI запускаемым проектом. Полное решение требует установленной поддержки MAUI; Desktop-решение собирается без неё.

Команды из корня репозитория:

```powershell
dotnet build HomeWork3/prjPasswordCreator/prjPasswordCreator.Desktop.slnx
dotnet run --project HomeWork3/prjPasswordCreator/cnsPasswordCreator
dotnet run --project HomeWork3/prjPasswordCreator/wfaPasswordCreator
dotnet run --project HomeWork3/prjPasswordCreator/wpfPasswordCreator
dotnet run --project HomeWork3/prjPasswordCreator/PasswordCreator.Tests
```

Примеры Console:

```powershell
dotnet run --project HomeWork3/prjPasswordCreator/cnsPasswordCreator -- --help
dotnet run --project HomeWork3/prjPasswordCreator/cnsPasswordCreator -- --length 24 --count 3 --groups luds
dotnet run --project HomeWork3/prjPasswordCreator/cnsPasswordCreator -- --length 6 --count 5 --groups d --output passwords.txt
```

`l` — строчные, `u` — прописные, `d` — цифры, `s` — спецсимволы. Все сообщения о результатах проверки и ошибках идут в stderr, сами пароли — в stdout. Некорректные параметры дают код завершения 1.

### MAUI

Установите workload `maui` (`dotnet workload install maui`) либо поддержку .NET MAUI в Visual Studio. Для Android нужны Android SDK/JDK и эмулятор или устройство. Для iOS/Mac Catalyst нужен Mac с соответствующими SDK. [Документация Microsoft по единому MAUI-проекту](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/single-project?view=net-maui-10.0).

Windows:

```powershell
dotnet build HomeWork3/prjPasswordCreator/mauiPasswordCreator/mauiPasswordCreator.csproj -f net10.0-windows10.0.19041.0 -p:TargetFrameworks=net10.0-windows10.0.19041.0
dotnet build HomeWork3/prjPasswordCreator/mauiPasswordCreator/mauiPasswordCreator.csproj -t:Run -f net10.0-windows10.0.19041.0 -p:TargetFrameworks=net10.0-windows10.0.19041.0
```

Android:

```powershell
dotnet build HomeWork3/prjPasswordCreator/mauiPasswordCreator/mauiPasswordCreator.csproj -f net10.0-android -p:TargetFrameworks=net10.0-android
dotnet build HomeWork3/prjPasswordCreator/mauiPasswordCreator/mauiPasswordCreator.csproj -t:Run -f net10.0-android -p:TargetFrameworks=net10.0-android
```

## Проверка перед сдачей

`./HomeWork3/verify.ps1` собирает Desktop-решение и запускает проверки без загрузки NuGet-пакетов. Тесты охватывают 15 комбинаций групп, 1500 случайных паролей, обязательные символы, длину, количество, ошибки, профили и команды модели представления. Это самостоятельный исполняемый набор: запускайте через `dotnet run`, а не `dotnet test`.

В каждом UI: сгенерируйте стандартный пакет; выберите только цифры; проверьте копирование и TXT; снимите все группы и убедитесь в сообщении об ошибке; задайте длину 3 при четырёх группах; очистите результаты. В WPF/MAUI также введите текст вместо числа: генерация должна остановиться с сообщением об ошибке.

Фактические результаты текущей проверки записаны в [VERIFICATION.md](VERIFICATION.md).
