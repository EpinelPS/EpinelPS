// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// EpinelPS管理控制台API通用工具函数

function runCmd(cmdName, cb, p1, p2)
{
    fetch("/adminapi/RunCmd", {
        method: "POST",
        body: JSON.stringify({
          cmdName: cmdName,
          p1: p1,
          p2: p2
        }),
        headers: {
          "Content-type": "application/json; charset=UTF-8",
          "Authorization": `Bearer ${localStorage.getItem('token')}`
        }
      })
      .then((response) => response.json())
  .then((json) => cb(json)).catch((error) => {
    console.error("命令执行失败:", error);
    alert("操作失败: " + error.message);
  });
}

function runSimpleCmd(cmdName, p1, p2)
{
  runCmd(cmdName, function(json){
    if (json.ok)
      alert("Operation completed");
    else
      alert("Error: " + json.error);
  }, p1, p2);
}

function runSimpleCmdWithPr(cmdName, p1, p2Title)
{
  let p2 = prompt(p2Title);
  if (p2 === undefined || p2 == null || p2 == "") return;
  runSimpleCmd(cmdName, p1, p2);
}
