# Proyecto de Encuestas - API

API de cuestionarios. Usé **.NET 6.0** y **SQLite** para la base de datos. La API permite crear diferentes tipos de preguntas y registrar respuestas a esas preguntas. Aunque en las especificaciones no se mencionaba, planeo agregar un campo `idUsuario` a las respuestas en el futuro para saber quién está respondiendo.

## Tipos de Preguntas

La API soporta tres tipos de preguntas:

- **SingleSelectQuestion** (Pregunta con opciones de selección única)
- **MultiSelectQuestion** (Pregunta con múltiples opciones seleccionables)
- **StarRatingQuestion** (Pregunta con calificación en estrellas)

Solo he probado a enviar respuestas para el tipo **SingleSelectQuestion**.

## Endpoints

### Crear una pregunta (PUT)

- **URL**: `/question/{id:guid}`
- **Método**: `PUT`
- **Cuerpo de la solicitud (JSON)**:

    Ejemplo de **SingleSelectQuestion**:

    ```json
    {
      "Title": "¿Cuál es tu color favorito?",
      "Type": "SingleSelectQuestion",
      "Options": ["Rojo", "Azul", "Verde"]
    }
    ```

### Enviar una respuesta (POST)

- **URL**: `/response`
- **Método**: `POST`
- **Cuerpo de la solicitud (JSON)**:

    Ejemplo de respuesta:

    ```json
    {
      "QuestionId": "123E4567-E89B-12D3-A456-426614174000",
      "Answer": "Rojo"
    }
    ```

## Tecnologías usadas

- **.NET 6.0** para la creación de la API.
- **SQLite** como base de datos.
- **Entity Framework Core** para interactuar con la base de datos.
- **VSCode** y la extensión **REST Client** para probar los endpoints.

## Notas

- He dejado las preguntas y respuestas que he usado para probar en la base de datos.
- Para probar los endpoints, utilicé la extensión **REST Client** de VSCode.

## Planeo hacer algunos cambios en el futuro

- Agregar un campo `idUsuario` a las respuestas para saber quién está respondiendo a las preguntas. Aunque eso no se menciona en las especificaciones, creo que sería útil para futuras versiones del sistema.
- Realizar modificaciones en el makefile.
