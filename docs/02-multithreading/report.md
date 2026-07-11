# Answer to Multithreading

## 目录

[TOC]

## 问答题

### (Q2.1)

+ `_items` 和 `_isCompleted`。通过 `lock (_items)`。

+ `_currentDirectory`、`_isAnalyzing`、`_logFiles` 和 `_analysisResults`。通过 `lock (_syncRoot)`。

+ 加入用 `if` 判断，保持 `producer` 不变，考察 `consumer` 的判断条件改为 `if`：

  ```c
  void producer() {
      while (1) {
          produce();
          lock(&mtx);
          buffer += 1;
          signal(&cv);
          unlock(&mtx);
      }
  }
  
  void consumer()
  {
      while (1)
      {
          lock(&mtx);
          if (buffer == 0) {
              wait(&cv, &mtx);
          }
          buffer -= 1;
          unlock(&mtx);
          consume();
      }
  }
  ```

  假设此时 `producer` 未锁住 `mtx`（例如已经 `unlock`，或处于 `produce` 阶段），仓库中商品数量为零（`buffer = 0`），有一个 `consumer` 在 `wait` 处等待新的商品，此时发生了虚假唤醒，`consumer` 从 `wait` 中醒来并重新锁住 `mtx`，由于未使用 `while` 判断，因此直接进行了 `buffer -= 1` 语句，导致 `buffer` 的值变成了 `-1`，违反了仓库中商品数应当非负的约束条件，导致程序逻辑错误。

### (Q2.2)

下面代码用于扫描 `*.log` 文件：

```csharp
var logFiles = Directory.EnumerateFiles(directoryPath, "*.log", SearchOption.TopDirectoryOnly)
    .Select(filePath => Path.GetFileName(filePath))
    .OrderBy(fileName => fileName);
```

或更精确地是这一段表达式：

```csharp
Directory.EnumerateFiles(directoryPath, "*.log", SearchOption.TopDirectoryOnly)
```

若要递归扫描，需要把 `SearchOption.TopDirectoryOnly` 改成 `SearchOption.AllDirectories`。

### (Q2.3)

略。