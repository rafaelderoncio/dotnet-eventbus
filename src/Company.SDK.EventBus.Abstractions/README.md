# Company.SDK.EventBus

O **Company.SDK.EventBus** é uma biblioteca que fornece um **barramento de eventos abstrato** para aplicações .NET.
Ele define contratos, classes base e um gerenciador de assinaturas em memória, permitindo que eventos sejam publicados e processados por manipuladores de forma organizada e desacoplada.

---

## 🎯 Objetivo

O projeto tem como finalidade:

* Prover uma **infraestrutura de mensageria desacoplada**.
* Padronizar a **publicação** e **consumo** de eventos dentro da aplicação.
* Permitir que diferentes implementações de barramento (ex.: RabbitMQ, Kafka, Azure Service Bus) utilizem a mesma abstração.
* Facilitar testes e simulações com um **gerenciador em memória**.

---

## 📂 Estrutura do Projeto

```
├── Company.SDK.EventBus.csproj
├── Events
│   └── Event.cs
├── Handlers
│   └── IEventHandler.cs
├── IEventBus.cs
├── Managers
│   ├── InMemorySubscriptionManager.cs
│   ├── ISubscriptionManager.cs
│   └── SubscriptionInfo.cs
```

* **Event**
  Classe base que representa qualquer evento dentro do sistema.
  Contém informações como `EventId`, `CreationDate`, `ReceiveCount` e `ErrorMessage`.

* **IEventHandler**
  Contrato para criação de manipuladores que processam eventos do tipo `TEvent`.

* **IEventBus**
  Interface principal do barramento. Define como publicar e assinar eventos de forma síncrona ou assíncrona.

* **SubscriptionInfo**
  Classe que descreve a relação entre um evento e seu manipulador.

* **ISubscriptionManager**
  Contrato para gerenciar quais eventos estão relacionados a quais manipuladores.

* **InMemorySubscriptionManager**
  Implementação em memória do `ISubscriptionManager`, útil para testes e cenários sem dependência externa.

---

## ⚙️ Conceitos Principais

### Evento (`Event`)

* Representa uma mensagem ou fato que ocorreu no sistema.
* Sempre possui um **identificador único** e uma **data de criação**.
* Pode registrar o número de vezes que foi processado (retry) e mensagens de erro.

### Manipulador (`IEventHandler`)

* Define como um evento específico será processado.
* Cada manipulador deve implementar a lógica dentro do método `Handle`.

### Barramento (`IEventBus`)

* Responsável por orquestrar a comunicação.
* Permite **publicar** eventos para o sistema e **assinar** eventos para processamento.
* Suporta chamadas síncronas e assíncronas.

### Gerenciamento de Assinaturas

* Feito pelo `ISubscriptionManager`.
* Permite registrar e remover manipuladores associados a eventos.
* A implementação padrão em memória (`InMemorySubscriptionManager`) é simples e voltada para uso local.

---

## 🚀 Benefícios

* **Desacoplamento** → Produtores e consumidores de eventos não precisam se conhecer.
* **Testabilidade** → Fácil simulação com a versão em memória.
* **Extensibilidade** → Pode ser integrado a diferentes brokers (RabbitMQ, Kafka, Azure Service Bus, etc.) mantendo a mesma abstração.
* **Padronização** → Uso consistente em toda a aplicação.

---

## 📌 Observações

* O projeto é **abstrato**: não fornece integração direta com um broker de mensageria.
* Implementações específicas (RabbitMQ, Kafka, etc.) podem herdar desse SDK para reutilizar os contratos.
* Ideal para projetos que precisam de uma camada de **infraestrutura de eventos reutilizável**.
