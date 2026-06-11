
async function fetchWithRetry(url, options = {}, retries = 3, delayMs = 500) {
    for (let attempt = 1; attempt <= retries; attempt++) {
        const response = await fetch(url, options);
        if (response.ok || response.status < 500)
            return response;
        if (attempt < retries)
            await new Promise(r => setTimeout(r, delayMs * attempt));
    }
    throw new Error("Request failed after " + retries + " attempts");
}

async function checkPasswordScore() {
    try {
        const password = document.querySelector("#password2").value
        const progress = document.querySelector("#passwordScore")
        const response = await fetchWithRetry('api/Password/PasswordScore', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(password)
        });

        if (!response.ok) {
            throw Error("error")
        }
        const data = await response.json();
        console.log(data)
        progress.value = data * 25
    }
    catch (error) {
        alert(error)
    }
}


async function getUserData() {
    try {
        const response = await fetchWithRetry('api/Users');
        if (!response.ok)
            throw new Error("error")

        else {
            const data = await response.json();
            alert(data);
        }
    }
    catch (e) {
        alert(e)
    }
}
   
async function Login() {
    try {
        const email = document.querySelector("#userName").value
        const password = document.querySelector("#password").value
        if (email == "" || password == "")
            throw Error("Please enter userName and password")
        const LoginUser = { email, password }
        const response = await fetchWithRetry('api/Users/Login', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(LoginUser)
        });
        if (response.status === 401) {
            alert("Email or password incorrect! Please try again.");
            return;
        }

        if (!response.ok) {
            const errData = await response.json().catch(() => null);
            const msg = errData?.message || errData?.innerException || "An unexpected error occurred.";
            throw new Error(msg);
        }

        const dataLogin = await response.json();
        console.log(dataLogin)
        sessionStorage.setItem('user', JSON.stringify(dataLogin))
        window.location.href = "Page2.html"
    }
    catch (error) {
        alert(error)
    }
}

async function Register() { 
    try {
        const Email = document.querySelector("#userName2").value;
        const FirstName = document.querySelector("#firstName").value;
        const LastName = document.querySelector("#lastName").value;
        const Password = document.querySelector("#password2").value;
        const data = { Email, FirstName, LastName, Password };
        if (Email == "" || Password == "")
            throw Error("Please enter user name and password")
        const response = await fetchWithRetry("api/Users", {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(data)
        });
        if (response.status == 400) {
            const responseText = await response.text()
            if (responseText == "Password")
                throw Error("Your password is too weak.")
            throw Error("Please try again")
        }
        if (!response.ok)
            throw Error("error")
        const dataRegister = await response.json();
        alert("success")
        console.log('POST Data:', dataRegister)
    }
    catch (e) {
        alert(e)
    }
    
}
function toggleNewUserForm() {
    document.querySelector('.newUser').classList.toggle('showNewUser');
    document.querySelector('.existUser').classList.toggle('showNewUser');
}

    