# HomeWork4 — prjPasswordCreator

Мохманд Марватулла, 241-3210.

Самостоятельная расширенная версия «Генератора паролей» по [заданию](https://docs.google.com/document/d/1-1PFM0usyQT2__cyrIiK5HKUTkArZ16aqhRvQ7hBEm0). Содержит собственные исходники всех проектов; HomeWork3 для сборки не нужен. Документ требований общий для обеих работ, поэтому новые возможности здесь являются выбранным развитием проекта.

## Выполнение требований

| Пункт документа | Реализация |
| --- | --- |
| Доработать lib/cns/wfa | Общая библиотека генерации, консольная и WinForms-версии с расширенными параметрами |
| Добавить WPF | Полноценный XAML-интерфейс с привязками и валидацией |
| Добавить MAUI | Мобильный XAML-интерфейс, Android/Windows, точки входа iOS/Mac Catalyst |
| Новые режимы | Случайный пароль, PIN-код, парольная фраза, свой алфавит |
| Новые настройки | Исключение похожих/заданных символов, отсутствие повторов, слова и разделитель фразы |
| Улучшить реализацию | Общая модель представления, история, JSON-настройки, обработка ошибок и автоматические проверки |

## Режимы

| Режим | Параметры и поведение |
| --- | --- |
| Случайный пароль | Длина 1–256, группы abc/ABC/123/!@#; каждый пароль содержит каждую выбранную группу |
| PIN-код | Только цифры, включая возможный ведущий 0; длина настраивается |
| Парольная фраза | 4–12 независимо выбранных английских слов, по умолчанию 10; разделитель 0–3 печатных символа |
| Свой алфавит | Печатные ASCII-символы без пробелов, повторяющиеся символы алфавита удаляются; длина 1–256 |

По умолчанию создаются 5 случайных паролей по 20 символов. За один раз можно создать 1–100 результатов. Профили: «Надёжный», «Без спецсимволов», «Только цифры», «Парольная фраза», «Свой алфавит».

Для символьных режимов можно исключить похожие `O0oIl1|`, любые указанные символы и запретить повтор символов внутри одного пароля. Полное исключение выбранной группы и длина сверх размера алфавита при запрете повторов дают понятную ошибку. Эти параметры не применяются к словам во фразах. Повторы слов и совпадения результатов в разных строках допустимы.

Словарь включён в библиотеку и не требует интернета. Это учебный словарь; оценка пространства фраз рассчитывается из его фактического размера. Для символьных режимов показана верхняя оценка `Length × log2(AlphabetSize)`; без повторов — сумма `log2(AlphabetSize − i)`. Обязательные группы и алгоритм снижают фактическую энтропию относительно этой оценки. Время взлома не прогнозируется.

Во всех режимах используется `RandomNumberGenerator.GetInt32`; символьные пароли перемешиваются Fisher–Yates. Библиотека не зависит от UI.

## История, настройки и экспорт

- WPF/WinForms/MAUI хранят до 50 последних паролей в памяти. «Очистить» удаляет результаты и историю сеанса. После закрытия приложения история исчезает.
- «Запомнить настройки» сохраняет только параметры генерации. Пароли, результаты и история в JSON не записываются.
- WPF/WinForms: `%LOCALAPPDATA%/prjPasswordCreator/HomeWork4/settings.json`; MAUI: `FileSystem.AppDataDirectory/settings.json`.
- При запуске настройки загружаются автоматически. Повреждённый JSON или ошибка доступа показываются пользователю; приложение остаётся работоспособным.
- Console читает JSON по явному `--settings FILE`; последующие аргументы переопределяют загруженные значения. Сохранение предпочтений доступно в графических приложениях.
- Копирование и экспорт выполняются пользователем. WPF/WinForms используют диалог сохранения, MAUI — системное меню «Поделиться». Экспортированный TXT содержит открытый текст; MAUI использует файл в локальном кэше экспорта.

## Структура и запуск

В `prjPasswordCreator` находятся `libPasswordCreator`, `PasswordCreator.Presentation`, `cnsPasswordCreator`, `wfaPasswordCreator`, `wpfPasswordCreator`, `mauiPasswordCreator`, `PasswordCreator.Tests`. Полное решение — `prjPasswordCreator.slnx`; решение без MAUI — `prjPasswordCreator.Desktop.slnx`.

Нужен .NET SDK 10; для WinForms/WPF — Windows. Откройте соответствующее `.slnx` в Visual Studio и назначьте запускаемый UI. Команды из корня репозитория:

```powershell
dotnet build HomeWork4/prjPasswordCreator/prjPasswordCreator.Desktop.slnx
dotnet run --project HomeWork4/prjPasswordCreator/cnsPasswordCreator
dotnet run --project HomeWork4/prjPasswordCreator/wfaPasswordCreator
dotnet run --project HomeWork4/prjPasswordCreator/wpfPasswordCreator
dotnet run --project HomeWork4/prjPasswordCreator/PasswordCreator.Tests
```

Примеры Console:

```powershell
dotnet run --project HomeWork4/prjPasswordCreator/cnsPasswordCreator -- --help
dotnet run --project HomeWork4/prjPasswordCreator/cnsPasswordCreator -- --length 24 --count 3 --groups luds --no-ambiguous --unique
dotnet run --project HomeWork4/prjPasswordCreator/cnsPasswordCreator -- --mode Pin --length 6 --count 5
dotnet run --project HomeWork4/prjPasswordCreator/cnsPasswordCreator -- --mode Passphrase --words 10 --separator - --count 2
dotnet run --project HomeWork4/prjPasswordCreator/cnsPasswordCreator -- --mode Custom --alphabet abcDEF012345 --length 10 --unique --count 4
dotnet run --project HomeWork4/prjPasswordCreator/cnsPasswordCreator -- --length 24 --exclude 'O0Il1' --output passwords.txt
```

Группы: `l` — строчные, `u` — прописные, `d` — цифры, `s` — спецсимволы. Значения режимов регистронезависимы. Пароли идут в stdout, сообщения и ошибки — в stderr. Успех возвращает 0, ошибка ввода/файла — 1.

### MAUI

Установите workload `maui` (`dotnet workload install maui`) или поддержку .NET MAUI в Visual Studio. Android требует Android SDK/JDK и устройство/эмулятор; Apple-платформы требуют Mac и соответствующие SDK. [Документация Microsoft](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/single-project?view=net-maui-10.0).

```powershell
# Windows: сборка и запуск
dotnet build HomeWork4/prjPasswordCreator/mauiPasswordCreator/mauiPasswordCreator.csproj -f net10.0-windows10.0.19041.0 -p:TargetFrameworks=net10.0-windows10.0.19041.0
dotnet build HomeWork4/prjPasswordCreator/mauiPasswordCreator/mauiPasswordCreator.csproj -t:Run -f net10.0-windows10.0.19041.0 -p:TargetFrameworks=net10.0-windows10.0.19041.0
# Android: сборка; для запуска добавьте -t:Run при подключённом устройстве/эмуляторе
dotnet build HomeWork4/prjPasswordCreator/mauiPasswordCreator/mauiPasswordCreator.csproj -f net10.0-android -p:TargetFrameworks=net10.0-android
```

## Проверка перед сдачей

`./HomeWork4/verify.ps1` собирает Desktop-решение и запускает автоматические проверки без NuGet-пакетов. Проверяются все группы и режимы, пределы параметров, исключения, отсутствие повторов, фразы, профили, обработка ошибок, ограничение истории и сохранение/чтение настроек. Тесты — отдельное исполняемое приложение: используйте `dotnet run`, а не `dotnet test`.

В каждом GUI проверьте все пять профилей; запрет повторов с длиной больше алфавита; полное исключение цифр при включённой группе цифр; копирование и TXT; сохраните настройки, перезапустите приложение; сгенерируйте более 50 результатов и очистите историю. При ручном редактировании JSON в некорректный формат приложение должно показать ошибку и позволить продолжить.

Фактические результаты текущей проверки записаны в [VERIFICATION.md](VERIFICATION.md).
