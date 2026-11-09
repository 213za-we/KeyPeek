# KeyPeek

一个常驻桌面的快捷键速查小窗。忘了某个 IDE 的组合键？按 `Ctrl+Alt+Space` 唤出来，敲两个字，看一眼，点一下就复制走了。

- 无边框悬浮窗，不占任务栏，位置和尺寸自动记住
- 内置 IntelliJ IDEA、VS Code、Eclipse、Sublime Text、PyCharm、WebStorm、Android Studio 七个平台的默认键位（Windows）
- 支持中文、别名、拼音首字母和全拼搜索（`zddq`、`duiqi`、`zi dong dui qi` 都能命中）
- 支持系统亚克力毛玻璃背景（Win11）
- 数据全部是纯 json，一个平台一份，随时自己改、自己加

![platform](https://img.shields.io/badge/platform-Windows-0078D4) ![runtime](https://img.shields.io/badge/WebView2-required-2ea44f)

---

## 快速开始

1. 下载仓库里的 `KeyPeek.exe`、三个 WebView2 DLL、`app\`、`packs\`，放进同一个文件夹（或直接解压 Release 里的压缩包）
2. 双击 `KeyPeek.exe`
3. 按 `Ctrl+Alt+Space` 唤出窗口

> 需要 WebView2 Runtime。Win10/11 一般已自带；如果双击一闪而过，去微软官网装一次 WebView2 Runtime 即可。

**必须保持的结构**：`KeyPeek.exe` + 三个 DLL + `app\` + `packs\` 必须在同一目录，缺一个就会打不开或界面空白。

---

## 使用

| 操作 | 效果 |
| --- | --- |
| `Ctrl+Alt+Space` | 显示 / 收起窗口（全局热键，任何程序里都能用） |
| 右上角 `×` | 收进系统托盘，不退出 |
| 托盘右键 | 显示 / 隐藏、切换尺寸、置顶开关、退出 |
| 拖动标题栏 | 移动窗口，位置自动记住；松手时靠近屏幕边缘会自动吸附 |

尺寸只有三档，从托盘里选：小窗 360×480、标准 440×620、大窗 560×760。不支持拖拽自由缩放——亚克力模糊面由系统按窗口尺寸合成，连续缩放会留下边界。

### 搜索

顶部两个输入项常驻，不用切模式：

- **动作**：支持中文、别名、拼音首字母、全拼
- **平台**：下拉选择全部平台或某一个平台

规则很简单：**任一为空就不按它过滤**。

- 两个都空 → 首页（平台入口 + 常用）
- 只填动作 → 全平台搜索
- 只选平台 → 该平台全部快捷键
- 两个都有 → 取交集

搜不到结果时不会一片空白，会额外列出 6 条最接近的条目，提示换个词。

### 键盘

| 键 | 作用 |
| --- | --- |
| `Ctrl+1` / `Ctrl+2`…`Ctrl+9` | 切到全部平台 / 依次切到各平台 |
| `Ctrl+F`、`Ctrl+K` | 跳到搜索框 |
| `↑` `↓` | 选中上一条 / 下一条 |
| `Enter` | 复制选中的键位 |
| `Esc` | 依次：关弹窗 → 关类型菜单 → 清搜索 → 清平台 → 收进托盘 |

### 条目

- 点整行 → 详情弹窗：大号键位、所属平台与分组、别名、完整说明，弹窗内可再复制
- 行尾的复制按钮 → 不弹窗，直接复制
- 复制次数自动累计，用于首页「常用」排序

### 类型筛选

平台页面包屑右侧有 `类型 ▾` 胶囊，点开是分组列表（搜索与导航、基础编辑、代码辅助、重构…），每项带条数。选完面包屑会变成「类型：重构」，胶囊变蓝底；换平台时自动清空。

### 首页

- **平台**：七个平台卡片，卡片上的「文档」标签可直达该平台官方键位文档
- **常用**：按真实复制次数排序，最多 12 条，可折叠，折叠状态会记住

### 主题

太阳图标循环切换 **浅色 → 深色 → 亚克力**。

- 亚克力用的是 Win11 系统 DWM 的 Acrylic 材质，窗口背后由系统实时模糊，页面叠一层半透明色。它不抓屏，所以拖动窗口不会闪
- 需要 WebView2 透明背景；个别显卡驱动下可能不透，程序会自动退回浅色并提示
- Win10 上直接退回浅色
- 配色 token 在 `app\theme.json`，改完重启生效

---

## 自定义数据

数据在 `packs\`，一个平台一份 json，格式 `shortcut-pack/1`：

```json
{
  "schema": "shortcut-pack/1",
  "platform": "idea",
  "name": "IntelliJ IDEA",
  "color": "#FE2857",
  "icon": "icons/idea.svg",
  "inherit": null,
  "os": "win",
  "keymap": "Windows 默认键位",
  "source": "https://www.jetbrains.com/zh-cn/help/idea/reference-keymap-win-default.html",
  "shortcuts": [
    {
      "id": "idea.format",
      "action": "格式化代码",
      "aliases": ["自动对齐", "对齐", "格式化", "reformat"],
      "keys": ["Ctrl+Alt+L"],
      "group": "代码辅助",
      "note": "整个文件或选中区域按代码风格重新缩进、换行"
    }
  ],
  "hidden": []
}
```

要点：

- `keys` 是**数组**：`["Ctrl+Alt+L"]` 是一次组合键，`["Ctrl+K","Ctrl+S"]` 是两段按键序列
- `aliases` 写得越口语，搜到它的概率越高
- `inherit` 可继承另一个平台（PyCharm / WebStorm / Android Studio 都继承 IDEA，各自只列差异条目）
- `source` 写了就会出现在首页平台卡片的「文档」入口
- 文件名以下划线开头（如 `_示例-空模板.json`）不会被加载，可当模板用

改完 json 后，在设置抽屉里点「重新加载」即生效，不用重启。程序每次写盘前会先存一份 `xxx.json.bak`。

### 导入现成配置

设置抽屉里的「导入配置 / 数据文件」支持：

| 格式 | 来源 |
| --- | --- |
| IDEA keymap `.xml` | IDEA `Settings → Keymap → 齿轮 → Export` |
| VS Code `keybindings.json` | `%APPDATA%\Code\User\keybindings.json` |
| CSV | 表头 `动作,键位,别名,分组,备注` |
| 通用 json | 即上面的 `shortcut-pack/1` |

导入前会先出预览（识别到多少条），确认后才写入。

---

## 常见问题

**双击没反应，或一闪而过？**
缺 WebView2 Runtime，或程序已经收在托盘里了（用 `Ctrl+Alt+Space` 唤一下）。

**改了 json 界面没变化？**
点设置抽屉里的「重新加载」。json 语法错误（少逗号、中文引号）会导致加载失败，按提示修。

**拼音搜不到新加的词？**
拼音表内联在 `app\KeyPeek.html` 的 `KP_PINYIN` / `KP_PYFULL`，只收录了数据里出现过的汉字。新增条目后如果拼音搜不到，把涉及的字补进这两个表即可。

**杀软报毒？**
自编译、没有代码签名，可能被误报。加白名单即可。

---

## 已知限制

- 数据是静态整理的官方默认键位，**不会读取也不会修改你本机的 IDE 配置**
- 内置七个平台：IDEA 123 条、VS Code 102 条、Eclipse 44 条、Sublime 42 条；PyCharm / WebStorm / Android Studio 继承 IDEA
- 只有一个目标：查和复制。没有收藏、没有最近查看，也没有测验、键位反查之类的附加功能
- 窗口尺寸只有三档
- 仅 Windows（依赖 WebView2）

---

## 开发者信息

- 外壳：WinForms + WebView2，单 exe，C#（`source\Launcher.cs`）
- 界面：单文件 `app\KeyPeek.html`，原生 JS，无构建步骤
- 编译器：Windows 自带的 `csc.exe`（`source\build.ps1`）

```powershell
Stop-Process -Name KeyPeek -ErrorAction SilentlyContinue
& 'source\build.ps1'
```

只改界面或数据不需要编译：直接改 `app\KeyPeek.html` 或 `packs\*.json`，重启程序生效。

---

## 许可

个人自用工具，随缘维护，欢迎自己改。
