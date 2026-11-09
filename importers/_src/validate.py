import os, json, re, collections

PACKS = r"E:\DouBao\0917\KeyPeek\packs"
for name in ("idea.json", "webstorm.json", "pycharm.json", "androidstudio.json", "vscode.json", "eclipse.json", "sublime.json"):
    j = json.load(open(os.path.join(PACKS, name), encoding="utf-8"))
    ids = [s["id"] for s in j["shortcuts"]]
    dup = [i for i, c in collections.Counter(ids).items() if c > 1]
    bad_types = [s["id"] for s in j["shortcuts"] if not isinstance(s["aliases"], list) or not isinstance(s["keys"], list)]
    empty_keys = [s["id"] for s in j["shortcuts"] if not s["keys"] or any(not k for k in s["keys"])]
    suspicious = [s["id"] for s in j["shortcuts"] if any(len(k) > 30 or k.endswith("+") or "  " in k for k in s["keys"])]
    groups = collections.Counter(s["group"] for s in j["shortcuts"])
    print("==", name, "条目", len(j["shortcuts"]), "重复id", len(dup), "类型异常", len(bad_types), "空键", len(empty_keys), "可疑键", len(suspicious))
    print("   分组:", dict(groups))
    if suspicious:
        print("   可疑:", [(s["id"], s["keys"]) for s in j["shortcuts"] if s["id"] in suspicious][:6])
