import os
from dotenv import load_dotenv
from sqlalchemy import create_engine, text
from sqlalchemy.engine import URL

from pathlib import Path

ROOT_DIR = Path(__file__).resolve().parent.parent
load_dotenv(ROOT_DIR / ".env")

url = URL.create(
    drivername="postgresql+psycopg2",
    username=os.getenv("DB_USER"),
    password=os.getenv("DB_PASSWORD"),
    host=os.getenv("DB_HOST"),
    port=int(os.getenv("DB_PORT")),
    database=os.getenv("DB_NAME"),
    query={"sslmode": "disable"}
)

print("Username:", repr(url.username))
print("Host:", url.host)
print("Port:", url.port)
print("Database:", url.database)

try:
    engine = create_engine(url)

    with engine.connect() as conn:
        result = conn.execute(
            text("SELECT current_database(), current_user")
        )
        print("DATABASE CONNECTION SUCCESSFUL")
        print(result.fetchone())

except Exception as e:
    print("DATABASE CONNECTION FAILED")
    print(type(e).__name__, str(e))