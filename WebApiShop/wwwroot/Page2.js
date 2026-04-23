const welcome = document.querySelector(".welcome");
const currentUser = JSON.parse(sessionStorage.getItem('user'));
welcome.textContent = `welcome back ${currentUser.firstName}`;





async function updateUser() {
    try {
        const Email = document.querySelector("#Email").value;
        const FirstName = document.querySelector("#FirstName").value;
        const LastName = document.querySelector("#LastName").value;
        const password = document.querySelector("#password").value;

        let currentUser = JSON.parse(sessionStorage.getItem('user'));
        if (!currentUser) {
            alert("No current user in sessionStorage");
            return;
        }

        const user = {
            email: Email,
            firstName: FirstName,
            lastName: LastName,
            password: password
        };

        const response = await fetch(`api/users/${currentUser.userId}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(user)
        });

        if (!response.ok) {
            throw new Error("Failed to update profile.");
        }

        sessionStorage.setItem('user', JSON.stringify({ ...currentUser, ...user }));

        alert("Profile updated successfully!");
    }
    catch (error) {
        alert("Error: " + error.message);
    }
}