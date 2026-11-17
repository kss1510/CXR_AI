
import os
from dotenv import load_dotenv
from sqlalchemy import create_engine, text
from sqlalchemy.engine import URL

from pathlib import Path

ROOT_DIR = Path(__file__).resolve().parent.parent
load_dotenv(ROOT_DIR / ".env")

url = URL.create(
    "postgresql+psycopg2",
    username=os.getenv("DB_USER"),
    password=os.getenv("DB_PASSWORD"),
    host=os.getenv("DB_HOST"),
    port=int(os.getenv("DB_PORT")),
    database=os.getenv("DB_NAME"),
    query={"sslmode": "disable"}
)

engine = create_engine(url)

with engine.connect() as conn:

    print("\n--- KNOWLEDGE CHUNKS VECTOR SEARCH ---")

    chunks = conn.execute(text("""
        SELECT
            id,
            source,
            LEFT(content, 150) AS preview,
            embedding <=> (
                SELECT embedding
                FROM public.knowledge_chunks
                WHERE embedding IS NOT NULL
                ORDER BY id
                LIMIT 1
            ) AS distance
        FROM public.knowledge_chunks
        WHERE embedding IS NOT NULL
        ORDER BY distance
        LIMIT 5
    """)).fetchall()

    for row in chunks:
        print(f"\nID: {row.id}")
        print(f"Source: {row.source}")
        print(f"Preview: {row.preview}")
        print(f"Cosine distance: {row.distance}")

    print("\n--- KNOWLEDGE BASE VECTOR SEARCH ---")

    entries = conn.execute(text("""
        SELECT
            id,
            title,
            category,
            embedding <=> (
                SELECT embedding
                FROM public.knowledge_base
                WHERE embedding IS NOT NULL
                ORDER BY id
                LIMIT 1
            ) AS distance
        FROM public.knowledge_base
        WHERE embedding IS NOT NULL
        ORDER BY distance
        LIMIT 5
    """)).fetchall()

    for row in entries:
        print(f"\nID: {row.id}")
        print(f"Title: {row.title}")
        print(f"Category: {row.category}")
        print(f"Cosine distance: {row.distance}")

print("\nVECTOR SEARCH TEST COMPLETE")
