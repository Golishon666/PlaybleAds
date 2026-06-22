import argparse
import base64
from pathlib import Path


def find_one(build_dir: Path, pattern: str) -> Path:
    matches = sorted(build_dir.rglob(pattern))
    if len(matches) != 1:
        raise RuntimeError(f"Expected one {pattern}, found {len(matches)}")
    return matches[0]


def encoded(path: Path) -> str:
    return base64.b64encode(path.read_bytes()).decode("ascii")


def main() -> None:
    parser = argparse.ArgumentParser(description="Pack a Unity WebGL build into one HTML file.")
    parser.add_argument("build_dir", type=Path)
    parser.add_argument("output_html", type=Path)
    args = parser.parse_args()

    loader = find_one(args.build_dir, "*.loader.js")
    data = find_one(args.build_dir, "*.data")
    framework = find_one(args.build_dir, "*.framework.js")
    wasm = find_one(args.build_dir, "*.wasm")

    payloads = {
        "loader": encoded(loader),
        "data": encoded(data),
        "framework": encoded(framework),
        "wasm": encoded(wasm),
    }

    html = f"""<!doctype html>
<html lang="en">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width,initial-scale=1,viewport-fit=cover,user-scalable=no">
  <title>Playable Ads Short</title>
  <style>
    html,body,#unity-container{{width:100%;height:100%;margin:0;overflow:hidden;background:#10281f}}
    #unity-canvas{{width:100%;height:100%;display:block;outline:0}}
    #progress{{position:fixed;left:15%;right:15%;bottom:8%;height:8px;background:#163f32;border-radius:4px}}
    #bar{{width:0;height:100%;background:#ffe13d;border-radius:4px;transition:width .12s linear}}
  </style>
</head>
<body>
  <div id="unity-container"><canvas id="unity-canvas" tabindex="-1"></canvas></div>
  <div id="progress"><div id="bar"></div></div>
  <script>
    const payloads = {payloads!r};
    function blobUrl(base64, type) {{
      const binary = atob(base64);
      const chunks = [];
      for (let offset = 0; offset < binary.length; offset += 1048576) {{
        const slice = binary.slice(offset, offset + 1048576);
        const bytes = new Uint8Array(slice.length);
        for (let i = 0; i < slice.length; i++) bytes[i] = slice.charCodeAt(i);
        chunks.push(bytes);
      }}
      return URL.createObjectURL(new Blob(chunks, {{type}}));
    }}
    const urls = {{
      loader: blobUrl(payloads.loader, "text/javascript"),
      data: blobUrl(payloads.data, "application/octet-stream"),
      framework: blobUrl(payloads.framework, "text/javascript"),
      wasm: blobUrl(payloads.wasm, "application/wasm")
    }};
    const script = document.createElement("script");
    script.src = urls.loader;
    script.onload = () => createUnityInstance(document.querySelector("#unity-canvas"), {{
      dataUrl: urls.data,
      frameworkUrl: urls.framework,
      codeUrl: urls.wasm,
      companyName: "Playable Ads Short",
      productName: "Playable Ads Short",
      productVersion: "1.0"
    }}, progress => {{
      document.querySelector("#bar").style.width = `${{progress * 100}}%`;
    }}).then(() => document.querySelector("#progress").remove())
      .catch(error => alert(error));
    document.body.appendChild(script);
  </script>
</body>
</html>
"""

    args.output_html.parent.mkdir(parents=True, exist_ok=True)
    args.output_html.write_text(html, encoding="utf-8")
    print(f"{args.output_html} ({args.output_html.stat().st_size} bytes)")


if __name__ == "__main__":
    main()
