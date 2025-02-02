# DQ (Do It Quickly) API

## Overview
# Rapid Prototyping for Web Applications (Dev Environment)

This project is designed to streamline the development process for web applications, focusing on rapid prototyping in a development environment. The core idea is to decouple backend development from the traditional Database-Backend-Frontend workflow, enabling parallel, non-blocking development.

## Development Workflow

1. **Database Development**:
   - The database developer writes the business logic within stored procedures, adhering to the **JSON In, JSON Out** approach. This ensures that the logic is encapsulated within the database and can be easily accessed.

2. **Frontend Development**:
   - The frontend developer directly interacts with the database logic by calling the stored procedures through an **Express endpoint**. This allows for immediate testing and iteration without waiting for backend implementation.

3. **Backend Development**:
   - Once the functionality and data models are finalized and tested between the database and frontend, the backend developer steps in. They create static models, controllers, and wrapper methods for the stored procedures, ensuring consistency with the already implemented database logic.

4. **Frontend Integration**:
   - Finally, the frontend developer switches from the temporary Express endpoint to the permanent backend endpoint, completing the integration process.

## Key Benefits
- **Parallel Development**: Frontend and backend development can happen simultaneously, reducing bottlenecks.
- **Rapid Iteration**: Changes can be tested quickly by directly interacting with the database logic.
- **Clear Separation of Concerns**: Each team (database, frontend, backend) focuses on their specific domain, improving collaboration and efficiency.

## Why This Approach?
This method is particularly useful for small to medium-sized projects where speed and flexibility are crucial.
By decoupling the backend from the initial development cycle, teams can prototype and test ideas faster,
ensuring that the final product aligns closely with business requirements.

## Getting Started
To use this approach in your project:
1. Set up your database and implement stored procedures using the **JSON In, JSON Out** pattern.
2. Once the frontend and database logic are finalized, implement the backend models and controllers.
3. Switch the frontend to use the backend endpoint for stage/production environment.

This workflow is ideal for teams looking to prototype quickly and iterate efficiently in a development environment.

## Technologies Used
- **Dapper**: A simple object mapper for .NET, known for its speed and efficiency, making it virtually as fast as using a raw ADO.NET data reader. It is an open-source, high-performance, lightweight, and flexible Micro ORM that is easy to use compared to other ORMs available in the market.
- **System.Text.Json**: The primary JSON serializer.
- **MediatR**: Implements the Mediator pattern for CQRS pattern realization.
- **FluentValidation**: Used for request validation.
- **Swashbuckle.AspNetCore**: Generates Swagger pages for RestAPI.

## Prerequisites

- !!!Critical Engineering Mindset!!!
- .NET 8.0
- Visual Studio 2022
- SQL Server 2019

## Testing
To run the tests, use the following command: dotnet test or Test Explorer in Visual Studio.
