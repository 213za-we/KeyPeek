import os, re, json, html

SRC = os.path.dirname(os.path.abspath(__file__))
PACKS = os.path.abspath(os.path.join(SRC, "..", "..", "packs"))

# ---------------- Android：官方 keyboard-shortcuts 页 ----------------
ANDROID_HTML = os.path.join(SRC, "android_keys.html")

def strip_tags(s):
    s = re.sub(r"<[^>]+>", "", s)
    return re.sub(r"\s+", " ", html.unescape(s)).strip()

SINGLE = re.compile(r"^(Esc|Enter|Tab|Space|Backspace|Delete|Insert|Home|End|F\d{1,2}|Page ?Up|Page ?Down|Up|Down|Left|Right)$")

def looks_like_keys(s):
    if len(s) > 40 or s in ("Action", "Tool window"):
        return False
    return "+" in s or bool(SINGLE.match(s))

def split_keys(s):
    s = s.replace("(backtick)", "").strip()
    s = re.sub(r"\s*\+\s*", "+", s)
    for a, b in (("Right Arrow", "Right"), ("Left Arrow", "Left"), ("Up Arrow", "Up"), ("Down Arrow", "Down")):
        s = s.replace(a, b)
    out = []
    for part in [p.strip() for p in re.split(r"\s+or\s+", s) if p.strip()]:
        if "+" not in part and out:
            mods = out[0].split("+")[:-1]
            part = "+".join(mods + [part]) if mods else part
        out.append(part)
    return out

def android_entries():
    raw = open(ANDROID_HTML, encoding="utf-8", errors="replace").read()
    rows = []
    for m in re.finditer(r"<table[\s\S]*?</table>", raw):
        for tr in re.finditer(r"<tr[\s\S]*?</tr>", m.group(0)):
            tds = [strip_tags(td.group(0)) for td in re.finditer(r"<td[\s\S]*?</td>", tr.group(0))]
            if len(tds) >= 2 and tds[0] and looks_like_keys(tds[1]):
                rows.append((tds[0], tds[1]))
    items, seen = [], set()
    for action, keys in rows:
        ks = split_keys(keys)
        if action.lower().startswith("comment/uncomment with line"):
            ks = ["Ctrl+/"]
        if action.lower().startswith("comment/uncomment with block"):
            ks = ["Ctrl+Shift+/"]
        sig = (action.lower(), tuple(ks))
        if sig in seen:
            continue
        seen.add(sig)
        items.append({"keys": ks, "desc": action})
    return items

# ---------------- 分组（仅用于界面分类，不影响数据本身） ----------------
RULES = [
    ("运行与调试", r"\b(run|debug|step|breakpoint|evaluate|resume|execution point|stop|rerun|build and run|apply changes)\b"),
    ("搜索与替换", r"\b(find|replace|search|occurrence|usages?|highlight)\b"),
    ("重构", r"\b(refactor|rename|extract|inline|introduce|surround|signature|safe delete|move|copy|duplicate|reformat|optimize imports|auto-indent|join|split|comment|toggle case|delete line|paste|select|override|implement|generate code)\b"),
    ("导航", r"\b(go to|navigate|jump|tab|bookmark|hierarchy|structure|declaration|implementation|symbol|class|file|line|tool window|favorites|recent|back|forward|project|version control|logcat|zoom)\b"),
    ("版本控制", r"\b(vcs|commit|update project|push|revert|changes?|history|shelf)\b"),
]

def classify(desc):
    low = desc.lower()
    for g, pat in RULES:
        if re.search(pat, low):
            return g
    return "编辑"

def slug(s):
    return (re.sub(r"[^a-z0-9]+", "-", s.lower()).strip("-") or "item")[:34]

def repair(keys, desc):
    out, low = [], desc.lower()
    for k in keys:
        if k.endswith("NumPad-") and k.count("NumPad") > 1:
            base = k.replace("NumPad-", "")
            out += [base, base[:-1] + "-"]
            continue
        if "comment" in low and k in ("Ctrl+", "Ctrl+Shift+"):
            k = "Ctrl+/" if k == "Ctrl+" else "Ctrl+Shift+/"
        out.append(k)
    return out

def make_entries(items, prefix):
    out, used = [], set()
    for it in items:
        base = "%s.%s" % (prefix, slug(it["desc"]))
        nid, i = base, 1
        while nid in used:
            i += 1
            nid = "%s-%d" % (base, i)
        used.add(nid)
        out.append({
            "id": nid,
            "action": it["desc"],
            "aliases": [],
            "keys": repair(it["keys"], it["desc"]),
            "group": it.get("group") or classify(it["desc"]),
            "note": "",
            "hot": False,
        })
    return out

WS_CARD = json.load(open(os.path.join(SRC, "webstorm_entries.json"), encoding="utf-8"))
PY_CARD = json.load(open(os.path.join(SRC, "pycharm_entries.json"), encoding="utf-8"))
ANDROID = android_entries()

ws_new = make_entries(WS_CARD, "ws.key")
py_new = make_entries(PY_CARD, "pyc.key")
as_new = make_entries(ANDROID, "as.key")
print("解析结果：webstorm %d / pycharm %d / android %d" % (len(ws_new), len(py_new), len(as_new)))

def load(name):
    return json.load(open(os.path.join(PACKS, name), encoding="utf-8"))

def fix(s):
    if not isinstance(s.get("aliases"), list):
        s["aliases"] = [] if s.get("aliases") in (None, "") else [str(s["aliases"])]
    if not isinstance(s.get("keys"), list):
        s["keys"] = [] if s.get("keys") in (None, "") else [str(s["keys"])]
    s.setdefault("note", "")
    s.setdefault("hot", False)
    return s

def save(name, obj):
    obj["shortcuts"] = [fix(s) for s in obj["shortcuts"]]
    with open(os.path.join(PACKS, name), "w", encoding="utf-8") as f:
        json.dump(obj, f, ensure_ascii=False, indent=2)

# ---- WebStorm：快捷键在前，模板在后；重复执行不会叠加 ----
ws = load("webstorm.json")
templates = [s for s in ws["shortcuts"] if not s["id"].startswith("ws.key.")]
ws["shortcuts"] = ws_new + templates
ws["keymap"] = "Windows 默认键位（官方键位卡 2026 版）"
ws["source"] = "https://resources.jetbrains.com/storage/products/webstorm/docs/WebStorm_ReferenceCard_Windows_Linux.pdf"
save("webstorm.json", ws)

# ---- PyCharm：官方键位卡，不再继承 ----
pyc = load("pycharm.json")
pyc["shortcuts"] = py_new
pyc["inherit"] = None
pyc["hidden"] = []
pyc["keymap"] = "Windows 默认键位（官方键位卡 2026 版）"
pyc["source"] = "https://resources.jetbrains.com/storage/products/pycharm/docs/PyCharm_ReferenceCard.pdf"
save("pycharm.json", pyc)

# ---- Android Studio：官方快捷键页 + 把 IDEA 的 Java/Kotlin 模板复制成自己的 ----
idea = load("idea.json")
tpl = [s for s in idea["shortcuts"] if s["group"].startswith("代码模板") and ("Java" in s["group"] or "Kotlin" in s["group"])]
as_tpl = []
for s in tpl:
    lang = "java" if "Java" in s["group"] else "kotlin"
    as_tpl.append(dict(s, id="as.tpl.%s.%s" % (lang, s["id"].split(".")[-1])))
print("复制 IDEA Java/Kotlin 模板 %d 条" % len(as_tpl))

asobj = load("androidstudio.json")
asobj["shortcuts"] = as_new + as_tpl
asobj["inherit"] = None
asobj["hidden"] = []
asobj["keymap"] = "Windows 默认键位（官方 keyboard shortcuts 页）"
asobj["source"] = "https://developer.android.com/studio/intro/keyboard-shortcuts"
save("androidstudio.json", asobj)

print("---")
for name in ("idea.json", "vscode.json", "eclipse.json", "sublime.json", "pycharm.json", "webstorm.json", "androidstudio.json"):
    j = load(name)
    tpl_n = len([s for s in j["shortcuts"] if s["group"].startswith("代码模板")])
    print("%-20s 总 %4d 条  模板 %3d  快捷键 %3d" % (name, len(j["shortcuts"]), tpl_n, len(j["shortcuts"]) - tpl_n))
