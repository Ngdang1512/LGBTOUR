#!/usr/bin/env python3
import argparse
import csv
import json
from pathlib import Path
from urllib import error, request


def post_json(url: str, payload: dict) -> tuple[int, str]:
    body = json.dumps(payload).encode("utf-8")
    req = request.Request(
        url,
        data=body,
        headers={"Content-Type": "application/json"},
        method="POST",
    )

    try:
        with request.urlopen(req, timeout=12) as resp:
            return resp.getcode(), resp.read().decode("utf-8", errors="ignore")
    except error.HTTPError as e:
        return e.code, e.read().decode("utf-8", errors="ignore")


def main() -> None:
    parser = argparse.ArgumentParser(description="Bulk register users via API")
    parser.add_argument("--api", default="http://localhost:5117", help="API base URL")
    parser.add_argument("--csv", required=True, help="CSV path with fullName,email,password")
    args = parser.parse_args()

    csv_path = Path(args.csv)
    if not csv_path.exists():
        raise SystemExit(f"CSV not found: {csv_path}")

    success = 0
    exists = 0
    failed = 0

    register_url = args.api.rstrip("/") + "/api/auth/register"

    with csv_path.open("r", encoding="utf-8-sig", newline="") as f:
        reader = csv.DictReader(f)

        for row in reader:
            full_name = (row.get("fullName") or "").strip()
            email = (row.get("email") or "").strip()
            password = (row.get("password") or "").strip()

            if not full_name or not email or not password:
                failed += 1
                continue

            status, text = post_json(
                register_url,
                {
                    "fullName": full_name,
                    "email": email,
                    "password": password,
                },
            )

            if 200 <= status < 300:
                success += 1
            elif status == 400 and "đã tồn tại" in text.lower():
                exists += 1
            else:
                failed += 1

    print(f"Done. created={success}, alreadyExists={exists}, failed={failed}")


if __name__ == "__main__":
    main()
