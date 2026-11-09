import json, os

PACKS = r"E:\DouBao\0917\KeyPeek\packs"

def load(n):
    return json.load(open(os.path.join(PACKS, n), encoding="utf-8"))

def save(n, o):
    with open(os.path.join(PACKS, n), "w", encoding="utf-8") as f:
        json.dump(o, f, ensure_ascii=False, indent=2)

# 1) 同一条目内的 keys 去重
for name in ("idea.json", "vscode.json", "eclipse.json", "sublime.json",
             "pycharm.json", "webstorm.json", "androidstudio.json"):
    j = load(name)
    changed = 0
    for s in j["shortcuts"]:
        if isinstance(s.get("keys"), list):
            uniq = []
            for k in s["keys"]:
                if k not in uniq:
                    uniq.append(k)
            if len(uniq) != len(s["keys"]):
                s["keys"] = uniq
                changed += 1
    save(name, j)
    if changed:
        print("去重 keys：", name, changed)

# 2) 补两条官方键位卡上被换行挤掉的常用项
ws = load("webstorm.json")
ids = {s["id"] for s in ws["shortcuts"]}
add = [
    ("ws.key.expand-all", "Expand all", ["Ctrl+Shift+NumPad+"], "编辑"),
    ("ws.key.collapse-all", "Collapse all", ["Ctrl+Shift+NumPad-"], "编辑"),
    ("ws.key.context-info", "Context Info", ["Alt+Q"], "编辑"),
]
templates = [s for s in ws["shortcuts"] if not s["id"].startswith("ws.key.")]
keys_new = [s for s in ws["shortcuts"] if s["id"].startswith("ws.key.")]
for nid, action, keys, group in add:
    if nid in ids:
        continue
    keys_new.append({"id": nid, "action": action, "aliases": [], "keys": keys,
                     "group": group, "note": "", "hot": False})
ws["shortcuts"] = keys_new + templates
save("webstorm.json", ws)
print("webstorm 现在", len(ws["shortcuts"]), "条")
