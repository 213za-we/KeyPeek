import json, glob, os

PACKS = r"E:\DouBao\0917\KeyPeek\packs"
tot = 0
for p in sorted(glob.glob(os.path.join(PACKS, "*.json"))):
    j = json.load(open(p, encoding="utf-8"))
    items = j.get("shortcuts", [])
    bad = [s.get("id") for s in items
           if not isinstance(s.get("aliases"), list) or not isinstance(s.get("keys"), list)]
    name = os.path.basename(p)
    if not name.startswith("_"):
        tot += len(items)
    top = ",".join(k for k in j if not k.startswith("_"))
    print("%-26s %4d 条  非数组 %d  %s" % (name, len(items), len(bad), top))
print("合计（正式包）:", tot)
