document.addEventListener("DOMContentLoaded", function () {
    loadJQueryAndInit();
});
function loadJQueryAndInit() {
    if (typeof jQuery === "undefined") {
        console.log("⏳ jQuery not found. Loading now...");
        let script = document.createElement("script");
        script.src = "https://code.jquery.com/jquery-3.6.0.min.js";
        script.onload = function () {
            console.log("✅ jQuery Loaded successfully.");
            $(document).ready(function () {
                initSearchFunctionality(); // ✅ รันหลัง jQuery โหลด
            });
        };
        script.onerror = function () {
            console.error("❌ Failed to load jQuery.");
        };
        document.head.appendChild(script);
    } else {
        console.log("✅ jQuery already loaded.");
        $(document).ready(function () {
            initSearchFunctionality(); // ✅ รันหลัง jQuery โหลด
        });
    }
}

$(document).ready(function () {
    $.ajax({
        url: "/api/user/check-auth",
        type: "GET",
        success: function (response) {
            console.log("✅ User is logged in.");
            $("#yourMeawBtn").prop("disabled", false); // ✅ เปิดใช้งานปุ่ม
        },
        error: function (xhr) {
            console.warn("❌ User is not logged in. Disabling 'Your Meaw' button...");
            $("#yourMeawBtn").prop("disabled", true); // ❌ ปิดใช้งานปุ่ม
            $("#yourMeawBtn").click(function (event) {
                event.preventDefault();
                alert("🔒 Please log in to view your meaws.");
                window.location.href = "/api/user/login"; // 🔄 Redirect ไปหน้า Login
            });
        }
    });
});


$(document).ready(function () {
    $("#loginForm").off("submit").on("submit", function (event) { // ✅ ป้องกันการ Bind ซ้ำ
        event.preventDefault();

        let loginDetails = {
            username: $("#Username").val(),
            password: $("#Password").val()
        };

        $.ajax({
            url: "/api/user/login",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(loginDetails),
            dataType: "json",
            success: function (response) {
                alert("🔓 Login successful!");
                window.location.href = "/";
            },
            error: function (xhr) {
                console.error("❌ Login failed:", xhr.responseText);
                alert(`❌ ${xhr.responseJSON?.message || "Invalid username or password"}`);
            }
        });
    });
    $("#logoutForm").click(function () {
        $.post("/api/user/logout", function () {
            alert("🚪 Logged out successfully!");
            window.location.href = "/api/user/login"; // ✅ Redirect ไปหน้า Login
        }).fail(function () {
            alert("❌ Logout failed. Try again.");
        });
    });
});
function viewCatDetails(catId) {
    console.log("🔍 Navigating to view page for cat ID:", catId);
    window.location.href = "/cats/view/" + catId; // 🔹 เปลี่ยน catId ตามค่าที่ส่งมา
}

function loadCatForEdit(catId) {
    console.log("🔍 Loading cat for edit with ID:", catId);

    $.ajax({
        url: "/api/Cats/cat/" + catId,
        type: "GET",
        dataType: "json",  // ✅ ขอ JSON response
        headers: { "Accept": "application/json" },
        success: function (data) {
            console.log("✅ Loaded Cat Data:", data);

            if (data.status === "Sold") {
                alert("❌ This meaw has already been sold. You can't edit it.");
                return;
            }

            // ✅ กรอกข้อมูลในฟอร์มแก้ไข
            $("#editCatId").val(data.id);
            $("#editBreed").val(data.breed);
            $("#editGender").val(data.gender);
            $("#editAge").val(data.age);
            $("#editPrice").val(data.price);
            $("#editCatFormContainer").slideDown();
        },
        error: function (xhr, status, error) {
            console.error("❌ Error loading cat:", xhr.responseText);
            alert("Error loading cat: " + xhr.responseText);
        }
    });
}

function updateCat() {
    let catId = $("#editCatId").val();

    let updatedCat = {};

    if ($("#editBreed").val()) updatedCat.Breed = $("#editBreed").val();
    if ($("#editGender").val()) updatedCat.Gender = $("#editGender").val();
    if ($("#editAge").val()) updatedCat.Age = parseInt($("#editAge").val()) || null;
    if ($("#editPrice").val()) updatedCat.Price = parseFloat($("#editPrice").val()) || null;

    let formData = new FormData();
    for (let key in updatedCat) {
        formData.append(key, updatedCat[key]);
    }

    // ตรวจสอบว่ามีไฟล์รูปภาพใหม่ไหม
    let fileInput = $("#editImage")[0].files;
    if (fileInput.length > 0) {
        formData.append("Image", fileInput[0]);
    }

    console.log("🚀 Sending Updated Cat Data:", updatedCat);

    $.ajax({
        url: "/api/Cats/update/" + catId,
        type: "PUT",
        data: formData,
        processData: false,
        contentType: false,
        success: function (response) {
            alert("✅ " + response);
            location.reload();
        },
        error: function (xhr, status, error) {
            alert("❌ Error updating meaw: " + xhr.responseText);
        }
    });
}



function deleteCat(catId, status) {
    if (status === "Sold") {
        alert("❌ This meaw has already been sold. You can't delete it.");
        return;
    }

    let confirmation = confirm("Are you sure you want to delete this meaw?");
    if (!confirmation) return;

    $.ajax({
        url: `/api/Cats/delete/${catId}`,
        type: "DELETE",
        xhrFields: { withCredentials: true }, // ✅ ให้เบราว์เซอร์ส่ง Cookie ไปกับคำขอ
        success: function (response) {
            alert("✅ Meaw deleted successfully!");
            window.location.href = "/Home/Index"; // ✅ กลับไปหน้าแรก
        },
        error: function (xhr, status, error) {
            console.error("❌ Delete Error:", xhr.responseText);
            alert("Error deleting meaw: " + xhr.responseText);
        }
    });
}


function buyCat(catId) {
    if (status === "Sold") {
        alert("❌ This meaw has already been sold. You can't buy it.");
        return;
    }
    let confirmation = confirm("Are you sure you want to buy this meaw?");
    if (!confirmation) return;
    

    $.ajax({
        url: "/api/Cats/buy/" + catId, // API ที่จะเรียก
        type: "POST", // ใช้ GET Request
        success: function (response) {
            alert("Hello human! I'm your meaw!"); // แสดงข้อความที่ได้จากเซิร์ฟเวอร์
            location.reload(); // รีโหลดหน้าเพื่ออัปเดตสถานะ
        },
        error: function (xhr, status, error) {
            alert("❌ Error purchasing meaw.This meaw has already been sold. ");
        }
    });
}



function initSearchFunctionality() {
    console.log("✅ jQuery is ready. Initializing search.js...");

    $(document).ready(function () {
        // ✅ Toggle Search Form เมื่อกด "Find Meaw"
        $("#findMeawBtn").click(function (event) {
            event.preventDefault();
            $("#searchCatFormContainer").slideToggle();
        });

        // ✅ Handle Search Cat Form Submission
        $("#searchCatForm").submit(function (event) {
            event.preventDefault();

            let breed = $("#searchBreed").val();
            let gender = $("#searchGender").val();
            let maxPrice = $("#searchMaxPrice").val();

            console.log("🔍 Searching for:", { breed, gender, maxPrice });

            $.ajax({
                url: "/api/Cats/search",
                type: "GET",
                data: { breed: breed, gender: gender, maxPrice: maxPrice },
                dataType: "json",
                success: function (data) {
                    console.log("✅ Search Results Received:", data);

                    if (!Array.isArray(data) || data.length === 0) {
                        alert("No meaw found T T");
                        $("#searchMessage").html("<p class='text-danger'>No meaw found T T</p>");
                        return;
                    } 
                    $("#catResults").empty();
                    data.forEach(cat => {
                        console.log("🐱 Processing Cat:", cat);

                        if (!cat.id) {
                            console.error("❌ ERROR: Missing cat ID", cat);
                        }
                            $("#catResults").append(`
                                <div class="col-md-4 mb-3">
                                    <div class="card shadow-sm">
                                        <img src="${cat.imageUrl}" class="card-img-top" alt="Cat Image">
                                        <div class="card-body text-center">
                                            <h5 class="card-title">${cat.breed}</h5>
                                            <p class="card-text"><strong>Gender:</strong> ${cat.gender}</p>
                                            <p class="card-text"><strong>Age:</strong> ${cat.age} months</p>
                                            <p class="card-text"><strong>Price:</strong> $${cat.price}</p>
                                            <a href="/api/cats/view/${cat.id}" class="view-detail-btn">View Details</a>
                                        </div>
                                    </div>
                                </div>
                            `);
                        });
                    
                },
                error: function (xhr, status, error) {
                    console.error("❌ Search Error:", status, error);
                    alert("An error occurred while searching.");
                    $("#searchMessage").html("<p class='text-danger'>An error occurred while searching.</p>");
                }
            });
        });

       

        // ✅ กด "Your Meaw" เพื่อโหลดแมวของตัวเอง
        $("#yourMeawBtn").click(function (event) {
            event.preventDefault();
            console.log("🐱 Fetching user's cats...");

            $.ajax({
                url: "/api/Cats/my-cats",
                type: "GET",
                dataType: "json",
                xhrFields: { withCredentials: true },
                success: function (data) {
                    console.log("✅ User's Cats Received:", data);
                    $("#catResults").empty();

                    if (!Array.isArray(data)) {
                        alert("🐱 " + (data.message || "No meaws found for this user."));
                        $("#catResults").append("<p class='text-center text-danger'>You don't own any cats.</p>");
                        return; // ✅ ออกจาก function ไม่ต้องรัน `.forEach()`
                    }
                    data.forEach(cat => {
                        $("#catResults").append(`
                                <div class="col-md-4 mb-3">
                                    <div class="card shadow-sm">
                                        <img src="${cat.image}" class="card-img-top" alt="Cat Image">
                                        <div class="card-body text-center">
                                            <h5 class="card-title">${cat.breed}</h5>
                                            <p class="card-text"><strong>Gender:</strong> ${cat.gender}</p>
                                            <p class="card-text"><strong>Age:</strong> ${cat.age} months</p>
                                            <p class="card-text"><strong>Price:</strong> $${cat.price}</p>
                                            <a href="/api/cats/view/${cat.id}" class="view-detail-btn">View Details</a>
                                        </div>
                                    </div>
                                </div>
                            `);
                    });
                    
                },
                error: function (xhr) {
                    console.error("❌ Error fetching user's cats:", xhr.responseText);

                    if (xhr.status === 401) {
                        alert("🔒 Please log in to view your meaws.");
                        window.location.href = "/api/user/login"; // 🔄 Redirect ไปหน้า Login
                    } else {
                        alert("❌ Error: Unable to fetch data. Please try again later.");
                    }
                }
            });
        });
        $("#addYourmeaw").off("click").on("click", function (event) {
            event.preventDefault();
            console.log("✅ Add Meaw Button Clicked"); // ✅ Debug
            $("#addCatFormContainer").slideToggle();
        });

        // ✅ Handle Add Cat Form Submission
        $("#addCatForm").off("submit").on("submit", function (event) {
            event.preventDefault();

            let formData = new FormData();
            formData.append("Breed", $("#breed").val());
            formData.append("Gender", $("#gender").val());
            formData.append("Age", $("#age").val());
            formData.append("Price", $("#price").val());
            formData.append("Image", $("#image")[0].files[0]);

            console.log("🐱 Adding Cat:", Object.fromEntries(formData));

            $.ajax({
                url: "/api/Cats/add",
                type: "POST",
                headers: {
                    "Authorization": "Bearer " + localStorage.getItem("token")
                },
                data: formData,
                processData: false,
                contentType: false,
                success: function (response) {
                    alert("Cat added successfully! 🎉");
                    $("#message").html("<p class='text-success'>Cat added successfully!</p>");
                    $("#addCatForm")[0].reset();
                    $("#addCatFormContainer").slideUp();
                },
                error: function (xhr, status, error) {
                    console.error("❌ Error adding cat:", xhr.responseText);
                    window.location.href = "/api/user/login"; // 🔄 Redirect ไปหน้า Login
                }
            });
        });

        // ✅ กด "All Meaws" เพื่อโหลดแมวทั้งหมด
        $("#allMeawBtn").click(function (event) {
            event.preventDefault();
            console.log("🐱 Fetching all cats...");

            $.ajax({
                url: "/api/Cats/all",
                type: "GET",
                dataType: "json",
                success: function (data) {
                    console.log("✅ All Cats Received:", data);
                    $("#catResults").empty();

                    if (!data || data.length === 0) {
                        alert("No meaws available.");
                        $("#catResults").append("<p class='text-center text-danger'>No meaws available at the moment.</p>");
                    } else {
                        data.forEach(cat => {
                            $("#catResults").append(`
                                <div class="col-md-4 mb-3">
                                    <div class="card shadow-sm">
                                        <img src="${cat.imageUrl}" class="card-img-top" alt="Cat Image">
                                        <div class="card-body text-center">
                                            <h5 class="card-title">${cat.breed}</h5>
                                            <p class="card-text"><strong>Gender:</strong> ${cat.gender}</p>
                                            <p class="card-text"><strong>Age:</strong> ${cat.age} months</p>
                                            <p class="card-text"><strong>Price:</strong> $${cat.price}</p>
                                            <a href="/api/cats/view/${cat.id}" class="view-detail-btn">View Details</a>
                                        </div>
                                    </div>
                                </div>
                            `);
                        });
                    }
                },
                error: function (xhr, status, error) {
                    console.error("❌ Error fetching all cats:", status, error);
                    alert("Error loading all meaws. Please try again.");
                }
            });
        });
    });
}


