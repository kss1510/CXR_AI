# CXR AI Knowledge Assistant

This repository contains a local knowledge-assistant system that combines semantic search, retrieval-augmented generation (RAG), and a Unity-based knowledge entry workflow. The goal is to help teams store troubleshooting knowledge, search it semantically, and generate grounded answers using a local LLM.

## What this project does

The project is designed around a simple idea:

1. Store knowledge entries such as problems, solutions, tags, and code snippets.
2. Convert that content into embeddings and search it semantically.
3. Use a local LLM to generate a grounded answer from the retrieved context.
4. Optionally capture and send knowledge from a Unity editor window to the backend.

In short, this is a mini enterprise knowledge base with AI-assisted retrieval.

---

## Main features

### 1. Knowledge entry management

You can create, read, update, and delete knowledge entries through a FastAPI backend.

Each entry may contain:

- title
- category
- problem
- solution
- code snippet
- tags

### 2. Semantic search

The backend generates embeddings for each entry and supports similarity-based search so users can find relevant knowledge even when the wording differs.

### 3. RAG-based answer generation

The system can take a user question and a set of retrieved documents, then build a grounded answer using a local model via Ollama.

### 4. Unity integration

The Unity project includes an editor window for entering knowledge and sending it to the backend API.

---

## Repository structure

- Backend/
  - main.py: FastAPI application entry point
  - database/: database connection and SQLAlchemy setup
  - models/: request/response and database models
  - semantic_search_engine/: embedding generation, chunking, ingestion, and vector search
  - rag_response_engine/: prompt building and local LLM response generation
- Unity/
  - Editor tools and scripts for collecting knowledge entries from Unity

---

## Technologies used

### Backend

- Python
- FastAPI
- SQLAlchemy
- PostgreSQL
- pgvector
- Sentence Transformers
- LangChain
- Ollama
- Pydantic

### Frontend / tooling

- Unity Editor
- C# editor scripts

---

## Prerequisites

Before running the project, make sure you have:

- Python 3.11+ (3.13 is recommended by the project config)
- PostgreSQL installed and running
- pgvector extension enabled in your database
- Ollama installed and running locally
- A local LLM model such as llama3 downloaded in Ollama
- Unity Editor if you want to use the Unity integration

---

## Dependencies to install

### Core Python dependencies

These are defined in the project configuration and are the main requirements for the backend:

- fastapi
- uvicorn
- sqlalchemy
- pydantic
- pydantic-settings
- python-dotenv
- sentence-transformers
- langchain
- langchain-text-splitters
- ollama
- pgvector
- psycopg2-binary

### Additional packages used by the semantic search module

These are used by the search engine submodule:

- pypdf
- beautifulsoup4

### Why each dependency is needed

- fastapi / uvicorn: run the API server
- sqlalchemy: ORM and database access
- pydantic: request and response validation
- python-dotenv: load environment variables from a .env file
- sentence-transformers: create embeddings for text
- pgvector / psycopg2-binary: store and query vector embeddings in PostgreSQL
- langchain / langchain-text-splitters: text splitting and retrieval-related workflows
- ollama: connect to local LLMs

---

## Quick start after cloning

### 1. Create and activate a Python virtual environment

```bash
cd CXR_AI
python3 -m venv .venv
source .venv/bin/activate
```

### 2. Install dependencies

If you use uv:

```bash
uv sync
```

If you prefer pip:

```bash
pip install -e .
```

You may also want to install the module-specific requirements directly if you plan to use the submodules separately:

```bash
pip install -r Backend/semantic_search_engine/Requirements.txt
pip install -r Backend/rag_response_engine/requirements.txt
```

### 3. Configure environment variables

Create a .env file in the project root or in the relevant backend folder with values similar to:

```env
DB_USER=postgres
DB_PASSWORD=your_password
DB_HOST=localhost
DB_PORT=5432
DB_NAME=knowledge_db

LLM_PROVIDER=ollama
LLM_MODEL=llama3
LLM_TEMPERATURE=0.2
LLM_NUM_CTX=8192
```

### 4. Prepare the database

Create a PostgreSQL database and enable the pgvector extension:

```sql
CREATE EXTENSION IF NOT EXISTS vector;
```

### 5. Start Ollama and pull a model

If Ollama is not installed yet, install it from https://ollama.com.

```bash
ollama pull llama3
ollama serve
```

### 6. Run the backend

From the project root:

```bash
uvicorn Backend.main:app --reload --host 0.0.0.0 --port 8000
```

The API will be available at:

- http://localhost:8000/
- http://localhost:8000/query
- http://localhost:8000/search

---

## How to use the API

### Create a knowledge entry

```bash
curl -X POST http://localhost:8000/query \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Image loading issue",
    "category": "Bug",
    "problem": "Images fail to load in the app.",
    "solution": "Use a streaming file reader and verify the file path.",
    "codeSnippet": "from PIL import Image",
    "tags": ["image", "bug", "python"]
  }'
```

### Search the knowledge base

```bash
curl -X POST http://localhost:8000/search \
  -H "Content-Type: application/json" \
  -d '{"query": "how do I fix an image loading issue"}'
```

### List entries

```bash
curl http://localhost:8000/query
```

---

## How to use the Unity integration

Open the Unity project from the Unity/ directory.

Then open:

- Tools > Knowledge Assistant

From there you can:

- enter a knowledge entry
- save it locally as JSON
- optionally send it to the backend API

If the backend is running on a different machine or port, update the API URL in the Unity editor script accordingly.

---

## Notes for developers

- The backend expects a working PostgreSQL instance with pgvector.
- The RAG pipeline depends on Ollama being available locally.
- If the semantic search or LLM components are not running correctly, the backend may return limited or empty context.
- The project is a prototype and is best suited for local development and experimentation.

---

## Summary

This repository is a local AI-powered knowledge assistant that brings together:

- structured knowledge storage
- semantic retrieval
- grounded AI answers
- Unity-based knowledge capture

It is a practical example of combining embeddings, vector search, and local LLM inference into a usable knowledge tool.
