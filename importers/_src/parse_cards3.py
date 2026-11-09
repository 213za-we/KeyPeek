import os, re, json

SRC = os.path.dirname(os.path.abspath(__file__))

KEYW = r"^(F\d{1,2}|Esc|Enter|Backspace|Delete|Insert|Space|Tab|Home|End|Page ?Up|Page ?Down|Up|Down|Left|Right|NumPad\+?|NumPad-|Double|BackQuote|Click|mouse|over|code|#|\[0-9\]|`|\[|\]|'|”)$"

def is_key(s):
    s = s.strip()
    if not s:
        return False
    parts = [p for p in re.split(r"\s+", s) if p]
    seen = False
    for p in parts:
        if p in ("+", "/", "-", "&", ",", "|"):
            continue
        if p in ("Ctrl", "Alt", "Shift", "Cmd", "Command", "Control", "Meta"):
            seen = True
            continue
        if re.match(KEYW, p) or re.match(r"^[0-9]$", p) or re.match(r"^[A-Z]$", p):
            seen = True
            continue
        return False
    return seen

def clean(t):
    return re.sub(r"\s+", " ", t.replace("\ufb01", "fi").replace("\ufb02", "fl")).strip()

def expand(keys):
    keys = keys.replace("NumPad + / -", "NumPad+ / NumPad-")
    keys = re.sub(r"\s*\+\s*", "+", keys)
    keys = re.sub(r"\s*/\s*", " / ", keys)
    out = []
    for part in keys.split(" / "):
        part = part.strip()
        if not part:
            continue
        if "+" not in part and out:
            mods = out[0].split("+")[:-1]
            part = "+".join(mods + [part]) if mods else part
        out.append(part)
    return out

RULES = [
    ("运行与调试", r"\b(run|debug|step|breakpoint|evaluate|resume|execution point|stop|rerun|deployment)\b"),
    ("搜索与替换", r"\b(find|replace|search|occurrence|usages?|highlight)\b"),
    ("重构", r"\b(refactor|rename|extract|inline|introduce|surround|signature|safe delete|move|copy|duplicate|reformat|optimize imports|auto-indent|join|split|comment|toggle case|delete line|paste|select)\b"),
    ("导航", r"\b(go to|navigate|jump|tab|bookmark|hierarchy|structure|declaration|implementation|symbol|class|file|line|tool window|favorites|recent|back|forward)\b"),
    ("版本控制", r"\b(vcs|commit|update project|push|revert|changes?|history|shelf)\b"),
]

def classify(desc):
    low = desc.lower()
    for g, pat in RULES:
        if re.search(pat, low):
            return g
    return "编辑"

BAD_DESC = re.compile(r"(jetbrains\.com|blog\.jetbrains|@|DEFAULT WINDOWS|To find any action inside|^finding a|^print)", re.I)

def parse(name, keys_first):
    lines = [clean(x) for x in open(os.path.join(SRC, name + ".txt"), encoding="utf-8").read().splitlines()]
    lines = [x for x in lines if x and not re.match(r"^\d+\s?$", x)]
    items = []
    if keys_first:
        pending = None
        for t in lines:
            if is_key(t):
                if pending:
                    items.append((pending, None))
                pending = t
            else:
                if pending:
                    items.append((pending, t))
                    pending = None
    else:
        pending = None
        for t in lines:
            if is_key(t):
                if pending:
                    items.append((t, pending))
                    pending = None
            else:
                pending = t
    out = []
    for keys, desc in items:
        if not desc or BAD_DESC.search(desc):
            continue
        d = desc.strip()
        if len(d) < 2 or d.isupper() or re.match(r"^[A-Z][A-Za-z /,&;]+$", d) and len(d.split()) <= 2 and "Center" in d:
            pass
        out.append({"keys": expand(keys), "desc": d, "group": classify(d)})
    # 去重（同键同描述）
    seen = set()
    uniq = []
    for e in out:
        sig = (tuple(e["keys"]), e["desc"])
        if sig in seen:
            continue
        seen.add(sig)
        uniq.append(e)
    return uniq

for name, kf in (("webstorm", True), ("pycharm", False)):
    items = parse(name, kf)
    print(name, len(items))
    for it in items[:14]:
        print("  [%s] %s -> %s" % (it["group"], "/".join(it["keys"]), it["desc"]))
    json.dump(items, open(os.path.join(SRC, name + "_entries.json"), "w", encoding="utf-8"), ensure_ascii=False, indent=1)
