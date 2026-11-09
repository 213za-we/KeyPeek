import os, urllib.request, fitz

SRC = os.path.dirname(os.path.abspath(__file__))
os.makedirs(SRC, exist_ok=True)

cards = {
    "webstorm": "https://resources.jetbrains.com/storage/products/webstorm/docs/WebStorm_ReferenceCard_Windows_Linux.pdf",
    "pycharm": "https://resources.jetbrains.com/storage/products/pycharm/docs/PyCharm_ReferenceCard.pdf",
}

for name, url in cards.items():
    pdf = os.path.join(SRC, name + ".pdf")
    if not os.path.exists(pdf):
        req = urllib.request.Request(url, headers={"User-Agent": "Mozilla/5.0"})
        with urllib.request.urlopen(req, timeout=60) as r, open(pdf, "wb") as f:
            f.write(r.read())
    doc = fitz.open(pdf)
    out = []
    for page in doc:
        blocks = page.get_text("blocks")
        blocks.sort(key=lambda b: (round(b[1] / 6), b[0]))
        for b in blocks:
            for line in b[4].splitlines():
                line = line.strip()
                if line:
                    out.append(line)
    txt = os.path.join(SRC, name + ".txt")
    with open(txt, "w", encoding="utf-8") as f:
        f.write("\n".join(out))
    print(name, "pdf", os.path.getsize(pdf), "lines", len(out))
    print("\n".join(out[:25]))
    print("----")
