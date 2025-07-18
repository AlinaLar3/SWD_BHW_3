Ларичева Алина Станиславовна 235

## Описание
Этот проект реализует микросервисное приложение для демонстрации функций управления счетами, создания заказа и проведения оплаты.

1.  **Order Service:** Отвечает за создание, просмотр списка заказов и обновление их статуса. Запуск создания заказа асинхронно запускает процесс оплаты (Kafka).
2.  **Payment Service:** Отвечает за управление пользовательскими счетами (создание, пополнение, просмотр баланса) + обработку платежей по заказам и публикует событие о статусе платежа (Kafka).
3.  **ApiGateway:** Действует как единая точка входа для клиентских приложений, маршрутизируя запросы к соответствующим внутренним микросервисам.

## Описание классов

### OrderService

*   **`OrdersController`**: Контроллер для управления заказами (создание нового заказа, просмотр списка всех заказов и получение информации об отдельном заказе).
*   **`AppDbContext`**: Класс `DbContext`, используемый для взаимодействия с базой данных `OrdersDb`. 
*   **`KafkaSettings`**: Класс для конфигурации Kafka.
*   **`CreateOrderRequest`**: DTO для запросов на создание заказа.
*   **`PaymentCompleted`**: DTO для сообщений о завершении платежа.
*   **`Order`**: Модель данных, представляющая заказ (его Id, Id владельца счёта, сумму, описание и его статус).
*   **`OutboxMessage`**: Модель данных для Transactional Outbox. Хранит информацию о сообщениях, которые должны быть отправлены в Kafka.
*   **`InboxProcessor`**: Служит для обработки входящих сообщений из Kafka и обновления статуса платежей.
*   **`OutboxProcessor`**: Служит для публикации сообщений в Kafka.

### PaymentService

*   **`AccountsController`**: Контроллер для управления пользовательскими счетами (создание счета, пополнения баланса, просмотр баланса).
*   **`AppDbContext`**: Класс `DbContext`, используемый для взаимодействия с базой данных `PaymentsDb`. 
*   **`KafkaSettings`**: Класс для конфигурации Kafka.
*   **`DepositRequest`**: DTO для запросов на пополнение счета`.
*   **`PaymentRequest`**: DTO для входящих запросов на оплату.
*   **`Account`**: Модель данных, представляющая счет пользователя (его Id и баланс).
*   **`InboxMessage`**: Модель данных для Transactional Inbox. Хранит информацию об уже обработанных входящих сообщениях Kafka.
*   **`OutboxMessage`**: Модель данных для Transactional Outbox. Хранит информацию о сообщениях, которые должны быть отправлены в Kafka.
*   **-`InboxProcessor`**: Служит для обработки входящих сообщений о создании заказа из Kafka. Обрабатывает платежи, создаёт OutboxMessage для отправки статуса оплаты.
*   **-`OutboxProcessor`**: Служит для публикации сообщений в Kafka.

### ApiGateway

*   **`Program.cs`**: Точка входа приложения, настройка сервисов.


### Инструкции:
Для запуска всего приложения вам понадобится Docker и Docker Compose.
1.  **Запуститите Docker (Docker Desktop)**
2.  **Перейдите в корневую директорию проекта**
3.  **Перейдите в папку kafka-stack-docker-compose проекта**
    ```bash
    cd kafka-stack-docker-compose
4.  **Выполните команду:**
    ```bash
    docker-compose -f full-stack.yml up -d
5. **Вернитесь в корневую директорию проекта**
   ```bash
    cd..
6.  **Выполните команду:**
    ```bash
    docker-compose up --build -d

### Порты:
    *   Доступ к Swagger: `localhost:5000/swagger`
    *   Панель управления Kafka: `localhost:8080`
