# Worldforge Test Execution And Handoff

## Purpose

Tai lieu nay chot quy trinh chay test truoc khi ban giao thay doi va cach ghi lai ket qua de phu hop voi spec `require-test-and-json-ut-report`.

## Standard Flow

1. Chon pham vi test phu hop voi thay doi:
   - Runtime/service logic thay doi trong mot module: uu tien `editmode` voi `-TestFilter` target vao assembly/test suite lien quan.
   - Unity runtime behavior, scene wiring, lifecycle, input, object cleanup: can nhac `playmode` hoac smoke test phu hop.
   - Chi sua doc/script van hanh: neu khong co automated test lien quan, phai ghi ro khoang trong kiem thu trong phan ban giao.
2. Chay test qua wrapper `Tools/Run-UnityTests.ps1`.
3. Lay ket qua tu console va file JSON artifact trong `../TestResults`.
4. Trong phan ban giao, luon ghi:
   - `Test scope`
   - `Status` (`passed` hoac `failed`)
   - `Artifacts` (duong dan JSON va, neu co, XML/log)
   - `Limitations` khi khong co test phu hop hoac test khong chay duoc

## Wrapper Script

Script: `Tools/Run-UnityTests.ps1`

Mac dinh:
- Tu tim `Unity.exe` moi nhat trong `C:\Program Files\Unity\Hub\Editor`
- Chay voi `-batchmode -nographics -quit`
- Luu artifact vao `C:\Project\WorldForge\TestResults`
- Neu project dang mo trong Unity Editor, wrapper se canh bao qua `preflightWarnings` va batch run co the that bai do project lock
- Tao artifact cho moi lan chay:
  - `*.xml`: unity/nunit structured result neu Unity tao duoc
  - `*.log`: Unity editor log cho lan chay
  - `*.stdout.log` va `*.stderr.log`: console output cua Unity batch run, huu ich khi `-logFile` khong chua day du ly do fail
  - `*.json`: manifest de tham chieu trong phan ban giao, kem `unityXmlSummary` neu co va `logSummary` khi runner bi chan boi project lock, compile error, hoac loi tu log

## Example Commands

Bootstrap flow regression:

```powershell
powershell -ExecutionPolicy Bypass -File ".\Tools\Run-UnityTests.ps1" `
  -Scope "bootstrap flow regression" `
  -TestPlatform editmode `
  -TestFilter "Worldforge.Core.Tests.ApplicationStartupFlowTests"
```

Targeted inventory editmode suite:

```powershell
powershell -ExecutionPolicy Bypass -File ".\Tools\Run-UnityTests.ps1" `
  -Scope "inventory targeted editmode" `
  -TestPlatform editmode `
  -TestFilter "Worldforge.Inventory.Tests"
```

PlayMode run:

```powershell
powershell -ExecutionPolicy Bypass -File ".\Tools\Run-UnityTests.ps1" `
  -Scope "scene smoke playmode" `
  -TestPlatform playmode
```

## Expected Handoff Format

Dung mau ngan gon sau trong phan tra loi cuoi:

```text
Test scope: bootstrap flow regression (editmode, filter: Worldforge.Core.Tests.ApplicationStartupFlowTests)
Status: passed
Artifacts: C:\Project\WorldForge\TestResults\20260723-220000-editmode-Worldforge.Core.Tests.ApplicationStartupFlowTests.json
Limitations: Unity chi xuat XML goc; JSON artifact la manifest tham chieu XML/log va tom tat ket qua.
```

Neu khong co test phu hop:

```text
Test scope: doc/script operational change only
Status: not run
Artifacts: none
Limitations: chua co automated test phu hop cho thay doi nay; da kiem tra bang review tinh hop le cua script/doc.
```

Neu test fail:

```text
Test scope: bootstrap flow regression (editmode, filter: Worldforge.Core.Tests.ApplicationStartupFlowTests)
Status: failed
Artifacts: C:\Project\WorldForge\TestResults\...\*.json
Limitations: xem log/XML trong artifact de lay chi tiet loi truoc khi ban giao.
```

## Notes

- Unity CLI hien tai trong repo nay da co bang chung su dung `-runTests` va `-testResults`, nhung artifact goc la XML thay vi JSON.
- JSON artifact duoc chuan hoa boi wrapper script de phuc vu handoff tracking, trong do tham chieu tro lai XML/log/stdout/stderr goc khi co.
- Khi Unity khong sinh XML, JSON artifact van ghi lai `notes` va co the kem `logSummary.failureCategory` de phan biet cac truong hop nhu `project_lock` hoac `compile_error`.
- Khong commit file trong `C:\Project\WorldForge\TestResults`; do la output cua moi lan chay.
