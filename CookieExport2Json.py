import json
import sys

# https://github.com/rotemdan/ExportCookies
# made because xitter hates the login function and you have to do this manually meow

def to_filetime(unix_ts):
    return (int(unix_ts) + 11644473600) * 10000000

def parse_netscape_cookies(filepath):
    cookies = []
    with open(filepath, "r") as f:
        for line in f:
            line = line.strip()
            if not line or line.startswith("#"):
                continue
            parts = line.split("\t")
            if len(parts) != 7:
                continue
            domain, include_sub, path, secure, expiry, name, value = parts
            unix_exp = int(float(expiry))
            entry = {
                "name": name,
                "value": value,
                "domain": domain,
                "path": path,
                "expiry": to_filetime(unix_exp),
                "secure": secure == "TRUE",
                "httpOnly": False,
                "ToSeleniumCookie": {
                    "name": name,
                    "value": value,
                    "domain": domain,
                    "path": path,
                    "secure": False,
                    "httpOnly": False,
                    "expiry": unix_exp
                }
            }
            cookies.append(entry)
    return cookies

def main():
    key_name    = sys.argv[1] if len(sys.argv) > 1 else "BOTNAME"

    input_file  = sys.argv[2] if len(sys.argv) > 2 else "cookies-x-com.txt"
    output_file = sys.argv[3] if len(sys.argv) > 3 else "cookies.json"

    cookies = parse_netscape_cookies(input_file)
    result = {key_name: json.dumps(cookies, indent=2)}

    with open(output_file, "w") as f:
        json.dump(result, f, indent=2)

    if key_name == "BOTNAME":
        print("please provide name for bot next time. using default bot key")

    print(f"Converted {len(cookies)} cookies → {output_file}")

if __name__ == "__main__":
    main()