window.onload = function () {

};

async function AdminLogin() {
    var username = document.getElementById("UsernameBox").value;
    var pw = document.getElementById("PasswordBox").value;

    let data = { username: username, password: pw };

    await fetch("/adminapi/login", {
        method: "POST",
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(data)
    }).then(async res => {
        console.log("Request complete! response:", res);

        var json = JSON.parse(await res.text());
        if (json.ok) {
            localStorage.setItem("token", res.token);

            window.location.pathname = "/admin/dashboard";
        }
        else {
            if (json.message !== undefined)
                $("#errormsg")[0].innerText = json.message;
            else
                $("#errormsg")[0].innerText = json.title;
        }
    });
}