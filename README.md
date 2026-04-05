# FreelanceMarket — Биржа фриланса

Полнофункциональное веб-приложение **«Биржа фриланса»** с Backend на **ASP.NET 8 Web API (Minimal APIs)** и Frontend на **Next.js (TypeScript, App Router, TailwindCSS)**.

Проект демонстрирует применение паттернов проектирования: **Singleton, Prototype, Builder + Director, Factory Method, Abstract Factory**, а также структурные паттерны **Facade, Adapter, Decorator, Composite**.

---

## 📁 Структура проекта

```
FreelanceMarket/
├── FreelanceMarket.sln                  # Solution файл
├── backend/
│   ├── FreelanceMarket.Domain/          # Слой домена (сущности, интерфейсы, паттерны)
│   │   ├── Entities/                    # User, Project, Proposal, Review
│   │   ├── Enums/                       # UserRole, ProjectStatus, ProposalStatus, ProjectType
│   │   ├── Interfaces/                  # IUserRepository, IProjectRepository, ...
│   │   └── Patterns/                    # Singleton, Prototype, Builder, Director, Factory Method
│   ├── FreelanceMarket.Application/     # Слой приложения (сервисы, DTO, валидация)
│   │   ├── Dtos/                        # DTO объекты
│   │   ├── Services/                    # AuthService, ProjectService, ProposalService, ReviewService
│   │   └── Validators/                  # FluentValidation валидаторы
│   ├── FreelanceMarket.Infrastructure/  # Инфраструктурный слой (EF Core, репозитории)
│   │   ├── Data/                        # AppDbContext
│   │   └── Repositories/               # Реализации репозиториев
│   └── FreelanceMarket.Api/            # Web API слой (Minimal APIs, JWT, Swagger)
│       ├── Auth/                        # JwtService
│       ├── Endpoints/                   # Auth, User, Project, Proposal, Review endpoints
│       ├── Extensions/                  # ClaimsPrincipalExtensions
│       └── Program.cs                   # Точка входа, DI, конфигурация
├── frontend/
│   ├── src/
│   │   ├── app/                         # Next.js App Router страницы
│   │   │   ├── layout.tsx               # Корневой layout
│   │   │   ├── page.tsx                 # Главная (каталог проектов)
│   │   │   ├── login/page.tsx           # Страница входа
│   │   │   ├── register/page.tsx        # Страница регистрации
│   │   │   ├── dashboard/page.tsx       # Личный кабинет
│   │   │   └── projects/
│   │   │       ├── create/page.tsx      # Создание проекта
│   │   │       └── [id]/page.tsx        # Детальная страница проекта
│   │   ├── components/                  # Компоненты (Navbar, ProjectCard, ThemeToggle)
│   │   ├── lib/                         # API клиент
│   │   ├── patterns/                    # Паттерны (ButtonFactory, ThemeAbstractFactory)
│   │   ├── store/                       # Zustand store (авторизация)
│   │   └── types/                       # TypeScript типы
│   ├── package.json
│   └── tailwind.config.js
└── README.md
```

---

## 🏗 Архитектура Backend

### Слои

| Слой | Назначение |
|------|-----------|
| **Domain** | Сущности, перечисления, интерфейсы репозиториев, реализация паттернов |
| **Application** | Сервисы бизнес-логики, DTO, валидаторы (FluentValidation) |
| **Infrastructure** | Entity Framework Core (InMemory DB), реализации репозиториев |
| **Api** | Minimal API endpoints, JWT аутентификация, Swagger, CORS |

### Основные сущности

- **User** — пользователь (Customer / Freelancer / Admin)
- **Project** — проект/задача (Open / InProgress / Completed / Cancelled)
- **Proposal** — отклик фрилансера на проект
- **Review** — отзыв/оценка (1–5)

### API Endpoints

| Метод | Путь | Описание |
|-------|------|----------|
| POST | `/api/auth/register` | Регистрация |
| POST | `/api/auth/login` | Вход |
| GET | `/api/users/me` | Текущий пользователь |
| GET | `/api/users/{id}` | Профиль пользователя |
| GET | `/api/users/{id}/reviews` | Отзывы о пользователе |
| GET | `/api/users/{id}/portfolio` | Публичное портфолио фрилансера (через адаптер внешнего профиля) |
| GET | `/api/projects` | Список проектов (фильтрация: status, maxBudget) |
| GET | `/api/projects/categories/tree` | Дерево категорий проектов |
| GET | `/api/projects/{id}` | Детали проекта |
| POST | `/api/projects` | Создание проекта (Builder + Director) |
| POST | `/api/projects/from-template` | Создание из шаблона (Prototype) |
| PUT | `/api/projects/{id}` | Редактирование проекта |
| PATCH | `/api/projects/{id}/assign` | Назначить фрилансера |
| PATCH | `/api/projects/{id}/complete` | Завершить проект |
| DELETE | `/api/projects/{id}` | Удалить проект |
| GET | `/api/projects/{projectId}/proposals` | Отклики на проект |
| POST | `/api/projects/{projectId}/proposals` | Создать отклик |
| PATCH | `/api/proposals/{id}/accept` | Принять отклик |
| PATCH | `/api/proposals/{id}/reject` | Отклонить отклик |
| POST | `/api/reviews` | Оставить отзыв |

---

## 🎨 Паттерны проектирования

### 1. Singleton — `SessionStateManager`

**Файл:** `backend/FreelanceMarket.Domain/Patterns/SessionStateManager.cs`

**Назначение:** глобальное потокобезопасное хранилище активных сессий пользователей. Реализован через `Lazy<T>` и `ConcurrentDictionary` для thread-safety.

**Почему Singleton:** единственная точка доступа к состоянию сессий без дублирования данных. В production можно обернуть в фасад к Redis.

**Где используется:** в `AuthService` при login/register для регистрации активной сессии.

### 2. Prototype — `ProjectTemplate` + `Project.Clone()`

**Файлы:**
- `backend/FreelanceMarket.Domain/Patterns/IPrototype.cs` — интерфейс
- `backend/FreelanceMarket.Domain/Patterns/ProjectTemplate.cs` — шаблон
- `backend/FreelanceMarket.Domain/Entities/Project.cs` — Project.Clone()

**Назначение:** клонирование типовых шаблонов проектов (лендинг, мобильное приложение, техподдержка) для быстрого создания нового проекта с предзаполненными полями.

**Где используется:** `ProjectService.CreateFromTemplateAsync()` — endpoint `POST /api/projects/from-template`.

### 3. Builder + Director — построение сложных проектов

**Файлы:**
- `backend/FreelanceMarket.Domain/Patterns/IProjectBuilder.cs` — интерфейс Builder
- `backend/FreelanceMarket.Domain/Patterns/ProjectBuilders.cs` — FixedPriceProjectBuilder, HourlyProjectBuilder, LongTermProjectBuilder
- `backend/FreelanceMarket.Domain/Patterns/ProjectDirector.cs` — Director

**Назначение:** Director управляет процессом создания проектов различных типов. Builder пошагово конфигурирует поля проекта (бюджет, дедлайн, навыки, тип).

**Где используется:** `ProjectService.CreateAsync()` — Director выбирает Builder по типу проекта.

### 4. Factory Method — создание уведомлений (Backend)

**Файлы:**
- `backend/FreelanceMarket.Domain/Patterns/Notifications.cs` — классы уведомлений
- `backend/FreelanceMarket.Domain/Patterns/NotificationFactories.cs` — фабрики

**Назначение:** каждый тип уведомления (отклик получен, отклик принят, проект завершён, отзыв получен) создаётся через отдельную фабрику. Это позволяет добавлять новые типы уведомлений без изменения существующего кода (Open/Closed Principle).

**Где используется:** `ProposalService`, `ReviewService` — при создании откликов и отзывов.

### 5. Factory Method — фабрика кнопок (Frontend)

**Файл:** `frontend/src/patterns/ButtonFactory.tsx`

**Назначение:** фабрика `createButton(variant)` возвращает React-компонент кнопки нужного типа (primary, secondary, danger, outline, ghost) с соответствующими стилями.

**Где используется:** на всех страницах — кнопки «Создать проект», «Откликнуться», «Завершить проект», фильтрация и т.д.

### 6. Abstract Factory — темизация (Frontend)

**Файл:** `frontend/src/patterns/ThemeAbstractFactory.tsx`

**Назначение:** `ThemeFactory` — абстрактная фабрика, создающая семейство тематизированных компонентов (Button, Card, Text, Badge, Input). `LightThemeFactory` и `DarkThemeFactory` — конкретные фабрики для светлой и тёмной темы.

**Где используется:** во всех компонентах через `useThemeFactory()`. Переключатель темы (next-themes) автоматически переключает фабрику.

### 7. Facade — единая точка доступа к API (Frontend)

**Файл:** `frontend/src/lib/apiFacade.ts`

**Назначение:** страницы и Zustand-store работают с фасадом верхнего уровня (`login`, `register`, загрузка проектов, создание откликов), а не напрямую с HTTP-клиентом. Фасад скрывает детали маршрутов, обработку ошибок и работу с JWT в localStorage.

**Где используется:** `authStore`, страницы `app/page.tsx`, `app/dashboard/page.tsx`, `app/projects/*`.

### 8. Adapter — интеграция внешнего профиля фрилансера (Backend)

**Файлы:**
- `backend/FreelanceMarket.Domain/Interfaces/IExternalFreelancerProfileAdapter.cs`
- `backend/FreelanceMarket.Infrastructure/Adapters/GithubFreelancerProfileAdapter.cs`

**Назначение:** адаптер приводит внешний GitHub-подобный DTO к внутренней модели `FreelancerPortfolioProfile`, которую ожидает API и фронтенд.

**Где используется:** endpoint `GET /api/users/{id}/portfolio` в `UserEndpoints`; на фронтенде данные выводятся на странице проекта для назначенного исполнителя.

### 9. Decorator — расширение карточек проектов (Frontend)

**Файл:** `frontend/src/components/decorators/ProjectCardDecorators.tsx`

**Назначение:** декораторы оборачивают базовую карточку проекта и добавляют поведение без изменения исходного компонента: подсветку срочных проектов и бейдж для проектов с высокой активностью.

**Где используется:** главная страница и дашборд отображают проекты через декорированную карточку.

### 10. Composite — дерево категорий проектов (Backend + Frontend)

**Файлы:**
- `backend/FreelanceMarket.Domain/Patterns/ProjectCategoryComposite.cs`
- `backend/FreelanceMarket.Application/Services/ProjectCategoryService.cs`
- `frontend/src/components/CategoryTree.tsx`

**Назначение:** единый интерфейс узла (`IProjectCategoryNode`) позволяет работать с листьями и составными категориями одинаково. На фронтенде дерево отображается рекурсивным компонентом.

**Где используется:** endpoint `GET /api/projects/categories/tree`; главная страница загружает и рендерит иерархию категорий.

---

## 🚀 Инструкция по запуску

### Предварительные требования

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/)
- npm или yarn

### Backend

```bash
cd backend/FreelanceMarket.Api
dotnet restore
dotnet run
```

API будет доступен на `http://localhost:5133`.
Swagger UI: `http://localhost:5133/swagger`.

### Frontend

```bash
cd frontend
npm install
npm run dev
```

Фронтенд будет доступен на `http://localhost:3000`.

---

## 🧪 Тестовый сценарий

### 1. Регистрация заказчика
- Открыть `http://localhost:3000/register`
- Заполнить: Имя = «Иван Заказчиков», Email = `ivan@test.com`, Пароль = `123456`, Роль = Заказчик
- Нажать «Зарегистрироваться» → перенаправление в личный кабинет

### 2. Создание проекта
- В личном кабинете нажать «Создать проект»
- **Вариант А** — ручное создание: заполнить название, описание, бюджет, тип, навыки → «Создать»
- **Вариант Б** — из шаблона (Prototype): выбрать «Из шаблона», выбрать «Разработка лендинга» → «Создать»

### 3. Регистрация фрилансера
- Выйти из аккаунта заказчика
- Зарегистрироваться как фрилансер: Имя = «Мария Кодерова», Роль = Фрилансер

### 4. Отклик на проект
- На главной странице найти проект Ивана
- Открыть детали проекта
- Заполнить сопроводительное письмо и ставку → «Откликнуться»

### 5. Принятие отклика
- Войти как Иван (заказчик)
- Открыть свой проект → раздел «Отклики»
- Нажать «Принять» напротив отклика Марии → статус проекта изменится на «В работе»

### 6. Завершение проекта
- Нажать «Завершить проект» → статус изменится на «Завершён»

### 7. Отзыв
- Через API (`POST /api/reviews`) оставить отзыв с оценкой 5 и комментарием

### 8. Смена темы
- Нажать иконку солнца/луны в навигации → тема переключится (Abstract Factory автоматически подстроит все компоненты)

---

## 🔧 Конфигурация

### База данных
По умолчанию используется **InMemory** база данных (для демо). Для production замените в `Program.cs`:

```csharp
// InMemory (текущая конфигурация)
builder.Services.AddDbContext<AppDbContext>(opts =>
    opts.UseInMemoryDatabase("FreelanceMarketDb"));

// PostgreSQL (production)
// builder.Services.AddDbContext<AppDbContext>(opts =>
//     opts.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
```

### JWT
Ключ, издатель и аудитория настраиваются в `appsettings.json`:
```json
{
  "Jwt": {
    "Key": "SuperSecretFreelanceMarketJwtKey2024VeryLongEnough!",
    "Issuer": "FreelanceMarket",
    "Audience": "FreelanceMarketClient",
    "ExpiresInMinutes": 1440
  }
}
```

### CORS
Frontend URL настроен на `http://localhost:3000` в `Program.cs`.

---

## 📋 Технологии

### Backend
- ASP.NET Core 8 (Minimal APIs)
- Entity Framework Core 8 (InMemory)
- JWT Authentication
- FluentValidation
- BCrypt.Net (хеширование паролей)
- Swagger / OpenAPI

### Frontend
- Next.js 14 (App Router)
- TypeScript
- TailwindCSS
- Zustand (state management)
- next-themes (переключение тем)
