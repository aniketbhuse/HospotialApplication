// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


setInterval(function () {
    const logo = document.getElementById("logoText");

    // Add animation class
    logo.classList.add("logo-animate");

    // Remove class after animation ends (so it can repeat)
    setTimeout(() => {
        logo.classList.remove("logo-animate");
    }, 1000);

}, 5000); // every 5 seconds


function showSection(sectionId) {
    const sections = document.querySelectorAll('.dynamic-section');

    sections.forEach(sec => {
        sec.style.display = 'none';
    });

    const activeSection = document.getElementById(sectionId);
    if (activeSection) {
        activeSection.style.display = 'block';
    }
}






// ❌ Delete (UI only)
function deleteCard(id) {
    const card = document.getElementById("card-" + id);
    if (card) {
        card.remove(); // removes from UI only
    }
}

// ✏️ Open Edit Modal
function openEditModal(id) {
    const card = document.getElementById("card-" + id);

    document.getElementById("editId").value = id;
    document.getElementById("editName").value = card.querySelector("h4").innerText;
    document.getElementById("editAge").value = card.querySelectorAll("p")[0].innerText.replace("Age: ", "");
    document.getElementById("editContact").value = card.querySelectorAll("p")[2].innerText.replace("Contact: ", "");
    document.getElementById("editDescription").value = card.querySelectorAll("p")[3].innerText.replace("Description: ", "");

    document.getElementById("editModal").style.display = "flex";
}

// 💾 Save Edit (UI only)
function saveEdit() {
    const id = document.getElementById("editId").value;
    const card = document.getElementById("card-" + id);

    card.querySelector("h4").innerText = document.getElementById("editName").value;
    card.querySelectorAll("p")[0].innerHTML = "<strong>Age:</strong> " + document.getElementById("editAge").value;
    card.querySelectorAll("p")[2].innerHTML = "<strong>Contact:</strong> " + document.getElementById("editContact").value;
    card.querySelectorAll("p")[3].innerHTML = "<strong>Description:</strong> " + document.getElementById("editDescription").value;

    closeModal();
}

// ❌ Close modal
function closeModal() {
    document.getElementById("editModal").style.display = "none";
}