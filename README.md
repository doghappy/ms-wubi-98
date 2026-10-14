# 微软五笔 98

Windows 自带的微软五笔默认使用 86 版五笔码表，对习惯 98 版或新世纪方案的用户不够友好。本项目以 https://github.com/yanhuacuo/98wubi-tables 为标准，将码表转换为微软输入法使用的 `ChsWubiNew.lex`，再替换系统中的同名词库，让微软五笔可以使用 98 方案输入。

本工具只替换词库文件，不会安装新的输入法，也不会修改 `ChsWubi.lex`。最终使用的方案和词库内容完全由输入给转换器的码表文件决定：当前附带的是 `98五笔含词表-【单义】.txt`，从 https://github.com/yanhuacuo/98wubi-tables 中下载得到，并非新世纪码表；如需新世纪方案，请自行寻找并准备对应的新世纪码表文件。

用户也可以自行修改码表文件，增删或调整词条来定制词库。简单来说，码表决定一切。

## 使用方法

建议从 [GitHub Releases](https://github.com/doghappy/ms-wubi-98/releases) 下载最新 Windows 发布包。发布包内含转换器、默认码表和替换脚本。

1. 将发布包解压到一个普通文件夹。
2. **先备份**系统原文件 `%windir%\InputMethod\CHS\ChsWubiNew.lex`，以便需要时恢复。
3. 先将输入法切换到非微软五笔。
4. 生成词库：

   发布包用户在解压目录打开终端，运行：
   ```powershell
   .\LexBuilder.exe "98五笔含词表-【单义】.txt"
   ```

   转换成功后，会在当前工作目录生成 `ChsWubiNew.lex`。
5. 以管理员身份打开 PowerShell，切换到该目录并运行替换脚本：

   ```powershell
   Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
   .\Replace-Wubi.ps1
   ```

   执行策略设置仅对当前 PowerShell 进程生效。脚本会取得系统词库的写入权限，并用当前目录中的 `ChsWubiNew.lex` 替换它。

## 恢复原词库

以管理员身份将备份的原文件复制回 `%windir%\InputMethod\CHS\ChsWubiNew.lex`。Windows 更新也可能还原系统词库；如更新后方案恢复为原样，可重新执行替换步骤。

## 已知问题

更换码表后，在微软五笔模式下使用拼音混输时，候选词弹出的五笔编码仍显示为 86 版编码。

## 使用其他方案或自定义词库

需要新世纪方案时，请自行寻找并准备新世纪码表文件；本项目提供的是 98 方案码表，不是新世纪码表。也可以自行编辑码表文件来定制词库。准备好符合下文格式的码表后，在发布包目录运行转换器，并将参数替换为所选码表的路径，例如：

```powershell
.\LexBuilder.exe "D:\path\to\your\table.txt"
```

码表须为 UTF-8 文本，每行格式为“词语<TAB>编码”；编码须为 1 到 4 个小写英文字母。`<TAB>` 表示一个实际的制表符。转换器生成的 `ChsWubiNew.lex` 会写入当前工作目录，之后按上文步骤替换系统词库即可。