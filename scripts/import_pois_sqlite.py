#!/usr/bin/env python3
import argparse
import csv
import sqlite3
from pathlib import Path
from typing import Optional


def parse_int(value: str, default: int) -> int:
    if value is None:
        return default
    value = value.strip()
    if value == "":
        return default
    return int(value)


def parse_float(value: str, default: float) -> float:
    if value is None:
        return default
    value = value.strip()
    if value == "":
        return default
    return float(value)


def parse_bool_to_int(value: str, default: int) -> int:
    if value is None:
        return default
    raw = value.strip().lower()
    if raw == "":
        return default
    if raw in {"1", "true", "yes", "y"}:
        return 1
    if raw in {"0", "false", "no", "n"}:
        return 0
    return default


def coalesce_text(value: Optional[str], default: Optional[str]) -> Optional[str]:
    if value is None:
        return default
    value = value.strip()
    return default if value == "" else value


def main() -> None:
    parser = argparse.ArgumentParser(description="Bulk import/update POIs into SQLite by Name")
    parser.add_argument("--db", required=True, help="Path to sqlite db")
    parser.add_argument("--csv", required=True, help="Path to CSV file")
    args = parser.parse_args()

    db_path = Path(args.db)
    csv_path = Path(args.csv)

    if not db_path.exists():
        raise SystemExit(f"DB not found: {db_path}")
    if not csv_path.exists():
        raise SystemExit(f"CSV not found: {csv_path}")

    conn = sqlite3.connect(str(db_path))
    conn.row_factory = sqlite3.Row

    inserted = 0
    updated = 0
    skipped = 0

    with conn, csv_path.open("r", encoding="utf-8-sig", newline="") as f:
        reader = csv.DictReader(f)

        for row in reader:
            name = (row.get("name") or "").strip()
            if not name:
                skipped += 1
                continue

            existing = conn.execute(
                "SELECT * FROM POIs WHERE Name = ? LIMIT 1",
                (name,),
            ).fetchone()

            if existing is None:
                description = coalesce_text(row.get("description"), None)
                lat = parse_float(row.get("lat", ""), 0.0)
                lng = parse_float(row.get("lng", ""), 0.0)
                radius = parse_int(row.get("radius", ""), 60)
                image = coalesce_text(row.get("image"), None)
                priority = parse_int(row.get("priority", ""), 1)
                is_stop_station = parse_bool_to_int(row.get("isStopStation", ""), 1)

                conn.execute(
                    """
                    INSERT INTO POIs (Name, Description, Lat, Lng, Radius, Image, Priority, IsStopStation)
                    VALUES (?, ?, ?, ?, ?, ?, ?, ?)
                    """,
                    (name, description, lat, lng, radius, image, priority, is_stop_station),
                )
                inserted += 1
                continue

            description = coalesce_text(row.get("description"), existing["Description"])
            lat = parse_float(row.get("lat", ""), float(existing["Lat"]))
            lng = parse_float(row.get("lng", ""), float(existing["Lng"]))
            radius = parse_int(row.get("radius", ""), int(existing["Radius"]))
            image = coalesce_text(row.get("image"), existing["Image"])
            priority = parse_int(row.get("priority", ""), int(existing["Priority"]))
            is_stop_station = parse_bool_to_int(row.get("isStopStation", ""), int(existing["IsStopStation"]))

            conn.execute(
                """
                UPDATE POIs
                SET Description = ?,
                    Lat = ?,
                    Lng = ?,
                    Radius = ?,
                    Image = ?,
                    Priority = ?,
                    IsStopStation = ?
                WHERE Id = ?
                """,
                (description, lat, lng, radius, image, priority, is_stop_station, int(existing["Id"])),
            )
            updated += 1

    print(f"Done. inserted={inserted}, updated={updated}, skipped={skipped}")


if __name__ == "__main__":
    main()
