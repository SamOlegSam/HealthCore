// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.
// Write your JavaScript code.

//-------Добавление пользователя--------------//
function AddLoginPassword() {
    $.ajax({
        url: "/Home/AddLoginPassword/",
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalContent').html(result);
            $('#ServicesModal').modal('show');
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }

    })
}

//-------Сохранение добавления пользователя----------//
function LoginPasswordSave() {

    var RT = document.getElementById('RT');

    var isValid = true;
    if ($('#Login').val() == "") {
        $('#Login').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#Login').css('border-color', 'lightgrey');
    }

    if ($('#Password').val() == "") {
        $('#Password').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#Password').css('border-color', 'lightgrey');
    }

    if ($('#idUS').val() == "0") {
        $('#idUS').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#idUS').css('border-color', 'lightgrey');
    }

    if (isValid == false) {
        return false;
    }

    //-------------------------------------------------------------------------------
    var selected = []
    var checkboxes = document.querySelectorAll('input[name=inList]:checked');

    for (var i = 0; i < checkboxes.length; i++) {
        selected.push(checkboxes[i].value);
    }
    //--------------------------------------------------------------------------------

    var data = {
        //'ID': ID,
        'Login': $('#Login').val(),
        'Password': $('#Password').val(),
        'EmployeeID': $('#idUS').val(),
        'UserModific': $('#UserModif').val(),
        'Rol': selected

    };

    console.log(data);
    $.ajax({
        url: "/Home/LoginPasswordSave/",
        type: "POST",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(data),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalContent').html(result);
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });

}
//--------------------------------//
// Удаление пользователя//

function DeleteUser(ID) {
    $.ajax({
        url: "/Home/DeleteUser/" + ID,
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalDeleteContent').html(result);
            $('#ServicesModalDelete').modal('show');
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });
}
//-----------------------------//
//Подтверждение удаления пользователя //
function DeleteUserOK(ID) {


    $.ajax({
        url: "/Home/DeleteUserOK/" + ID,
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalDeleteContent').html(result);
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });
}
//-----------------------------//
// Редактирование пользователя//

function UserEdit(ID) {
    $.ajax({
        url: "/Home/UserEdit/" + ID,
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalContent').html(result);
            $('#ServicesModal').modal('show');
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }

    })
}
//-------------------------//
// Сохранение редактирования пользователя//

function UserEditSave() {

    var RT = document.getElementById('RT');

    var isValid = true;

    if ($('#LoginEdit').val() == "") {
        $('#LoginEdit').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#LoginEdit').css('border-color', 'lightgrey');
    }

    if ($('#PasswordEdit').val() == "") {
        $('#PasswordEdit').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#PasswordEdit').css('border-color', 'lightgrey');
    }

    if ($('#idUSEdit').val() == "") {
        $('#idUSEdit').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#idUSEdit').css('border-color', 'lightgrey');
    }

    if (isValid == false) {
        return false;
    }

    var selected = []
    var checkboxes = document.querySelectorAll('input[name=inList]:checked');

    for (var i = 0; i < checkboxes.length; i++) {
        selected.push(checkboxes[i].value);
    }
    //--------------------------------------------------------------------------------

    var data = {

        'Id': $('#IDEdit').val(),
        'Login': $('#NameEdit').val(),
        'Password': $('#PasswordEdit').val(),
        'EmployeeID': $('#idUSEdit').val(),
        'UserModific': $('#UserModifEdit').val(),
        'Rol': selected

    };
    console.log(data);
    $.ajax({
        url: "/Home/UserEditSave/",
        type: "POST",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(data),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalContent').html(result);
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });
}
// Нажатие вкладки Справка в модальном окне //

function Reference(ID) {
    $.ajax({
        url: "/Home/Reference/",
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalContent').html(result);
            $('#ServicesModal').modal('show');
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }

    })
}
//---------------ДОЛЖНОСТИ-------------------------------------//
//-------Добавление должности--------------//
function AddPosition() {
    $.ajax({
        url: "/Home/AddPosition/",
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalContent').html(result);
            $('#ServicesModal').modal('show');
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }

    })
}

//-------Сохранение добавления должности----------//
function PositionSave() {

    var isValid = true;
    if ($('#Position').val() == "") {
        $('#Position').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#Position').css('border-color', 'lightgrey');
    }

    if (isValid == false) {
        return false;
    }

    var data = {
        //'ID': ID,
        'Login': $('#Position').val(),
        'Password': $('#Priznak').val(),
        'UserModific': $('#UserModif').val(),

    };

    $.ajax({
        url: "/Home/PositionSave/",
        type: "POST",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(data),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalContent').html(result);
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });

}
//--------------------------------//
// Удаление должности//

function DeletePosition(ID) {
    $.ajax({
        url: "/Home/DeletePosition/" + ID,
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalDeleteContent').html(result);
            $('#ServicesModalDelete').modal('show');
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });
}
//-----------------------------//
//Подтверждение удаления должности//
function DeletePositionOK(ID) {


    $.ajax({
        url: "/Home/DeletePositionOK/" + ID,
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalDeleteContent').html(result);
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });
}
//-----------------------------//
// Редактирование должности//

function PositionEdit(ID) {
    $.ajax({
        url: "/Home/PositionEdit/" + ID,
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalContent').html(result);
            $('#ServicesModal').modal('show');
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }

    })
}
//-------------------------//
// Сохранение редактирования должности//

function PositionEditSave() {

    var isValid = true;

    if ($('#NameEdit').val() == "") {
        $('#NameEdit').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#NameEdit').css('border-color', 'lightgrey');
    }

    if ($('#PriznakEdit').val() == "") {
        $('#PriznakEdit').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#PriznakEdit').css('border-color', 'lightgrey');
    }

    if (isValid == false) {
        return false;
    }

    var data = {

        'Id': $('#IDEdit').val(),
        'Login': $('#NameEdit').val(),
        'Password': $('#PriznakEdit').val(),
        'EmployeeID': $('#idUSEdit').val(),
        'UserModific': $('#UserModifEdit').val(),

    };
    console.log(data);
    $.ajax({
        url: "/Home/PositionEditSave/",
        type: "POST",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(data),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalContent').html(result);
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });
}
//------------------------------------------------------------//

//---------------ОТДЕЛЫ-------------------------------------//
//-------Добавление отдела--------------//
function AddDepartment() {
    $.ajax({
        url: "/Home/AddDepartment/",
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalContent').html(result);
            $('#ServicesModal').modal('show');
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }

    })
}

//-------Сохранение добавления отдела----------//
function DepartmentSave() {

    var isValid = true;
    if ($('#Department').val() == "") {
        $('#Department').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#Department').css('border-color', 'lightgrey');
    }

    if (isValid == false) {
        return false;
    }

    var data = {
        //'ID': ID,
        'Login': $('#Department').val(),
        'Password': $('#Priznak').val(),
        'UserModific': $('#UserModif').val(),

    };

    $.ajax({
        url: "/Home/DepartmentSave/",
        type: "POST",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(data),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalContent').html(result);
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });

}
//--------------------------------//
// Удаление отдела//

function DeleteDepartment(ID) {
    $.ajax({
        url: "/Home/DeleteDepartment/" + ID,
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalDeleteContent').html(result);
            $('#ServicesModalDelete').modal('show');
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });
}
//-----------------------------//
//Подтверждение удаления отдела//
function DeleteDepartmentOK(ID) {


    $.ajax({
        url: "/Home/DeleteDepartmentOK/" + ID,
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalDeleteContent').html(result);
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });
}
//-----------------------------//
// Редактирование отдела//

function DepartmentEdit(ID) {
    $.ajax({
        url: "/Home/DepartmentEdit/" + ID,
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalContent').html(result);
            $('#ServicesModal').modal('show');
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }

    })
}
//-------------------------//
// Сохранение редактирования отдела//

function DepartmentEditSave() {

    var isValid = true;

    if ($('#NameEdit').val() == "") {
        $('#NameEdit').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#NameEdit').css('border-color', 'lightgrey');
    }

    if ($('#PriznakEdit').val() == "") {
        $('#PriznakEdit').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#PriznakEdit').css('border-color', 'lightgrey');
    }

    if (isValid == false) {
        return false;
    }

    var data = {

        'Id': $('#IDEdit').val(),
        'Login': $('#NameEdit').val(),
        'Password': $('#PriznakEdit').val(),
        'EmployeeID': $('#idUSEdit').val(),
        'UserModific': $('#UserModifEdit').val(),

    };

    $.ajax({
        url: "/Home/DepartmentEditSave/",
        type: "POST",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(data),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalContent').html(result);
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });
}
//------------------------------------------------------------//

//---------------Подразделения-------------------------------------//
//-------Добавление подразделения--------------//
function AddFilial() {
    $.ajax({
        url: "/Home/AddFilial/",
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalContent').html(result);
            $('#ServicesModal').modal('show');
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }

    })
}

//-------Сохранение добавления подразделения----------//
function FilialSave() {

    var isValid = true;
    if ($('#Filial').val() == "") {
        $('#Filial').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#Filial').css('border-color', 'lightgrey');
    }

    if (isValid == false) {
        return false;
    }

    var data = {
        //'ID': ID,
        'Login': $('#Filial').val(),
        'Password': $('#Prefix').val(),
        'UserModific': $('#UserModif').val(),

    };

    $.ajax({
        url: "/Home/FilialSave/",
        type: "POST",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(data),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalContent').html(result);
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });

}
//--------------------------------//
// Удаление подразделения//

function DeleteFilial(ID) {
    $.ajax({
        url: "/Home/DeleteFilial/" + ID,
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalDeleteContent').html(result);
            $('#ServicesModalDelete').modal('show');
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });
}
//-----------------------------//
//Подтверждение удаления подразделения//
function DeleteFilialOK(ID) {


    $.ajax({
        url: "/Home/DeleteFilialOK/" + ID,
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalDeleteContent').html(result);
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });
}
//-----------------------------//
// Редактирование подразделения//

function FilialEdit(ID) {
    $.ajax({
        url: "/Home/FilialEdit/" + ID,
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalContent').html(result);
            $('#ServicesModal').modal('show');
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }

    })
}
//-------------------------//
// Сохранение редактирования подразделения//

function FilialEditSave() {

    var isValid = true;

    if ($('#NameEdit').val() == "") {
        $('#NameEdit').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#NameEdit').css('border-color', 'lightgrey');
    }

    if (isValid == false) {
        return false;
    }

    var data = {

        'Id': $('#IDEdit').val(),
        'Login': $('#NameEdit').val(),
        'Password': $('#PrefixEdit').val(),
        'UserModific': $('#UserModifEdit').val(),

    };

    $.ajax({
        url: "/Home/FilialEditSave/",
        type: "POST",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(data),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalContent').html(result);
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });
}
//------------------------------------------------------------//

//---------------Города-------------------------------------//
//-------Добавление города--------------//
function AddCity() {
    $.ajax({
        url: "/Home/AddCity/",
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalContent').html(result);
            $('#ServicesModal').modal('show');
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }

    })
}

//-------Сохранение добавления города----------//
function CitySave() {

    var isValid = true;
    if ($('#City').val() == "") {
        $('#City').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#City').css('border-color', 'lightgrey');
    }

    if ($('#idUS').val() == "0") {
        $('#idUS').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#idUS').css('border-color', 'lightgrey');
    }

    if (isValid == false) {
        return false;
    }

    var data = {
        //'ID': ID,
        'Login': $('#City').val(),
        'EmployeeID': $('#idUS').val(),
        'UserModific': $('#UserModif').val(),

    };

    $.ajax({
        url: "/Home/CitySave/",
        type: "POST",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(data),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalContent').html(result);
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });

}
//--------------------------------//
// Удаление города//

function DeleteCity(ID) {
    $.ajax({
        url: "/Home/DeleteCity/" + ID,
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalDeleteContent').html(result);
            $('#ServicesModalDelete').modal('show');
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });
}
//-----------------------------//
//Подтверждение удаления города//
function DeleteCityOK(ID) {


    $.ajax({
        url: "/Home/DeleteCityOK/" + ID,
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalDeleteContent').html(result);
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });
}
//-----------------------------//
// Редактирование города//

function CityEdit(ID) {
    $.ajax({
        url: "/Home/CityEdit/" + ID,
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalContent').html(result);
            $('#ServicesModal').modal('show');
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }

    })
}
//-------------------------//
// Сохранение редактирования города//

function CityEditSave() {

    var isValid = true;

    if ($('#NameEdit').val() == "") {
        $('#NameEdit').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#NameEdit').css('border-color', 'lightgrey');
    }

    if (isValid == false) {
        return false;
    }

    var data = {

        'Id': $('#IDEdit').val(),
        'Login': $('#CityEdit').val(),
        'EmployeeID': $('#idUSEdit').val(),
        'UserModific': $('#UserModifEdit').val(),

    };

    $.ajax({
        url: "/Home/CityEditSave/",
        type: "POST",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(data),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalContent').html(result);
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });
}
//------------------------------------------------------------//

//---------------СТРАНЫ-------------------------------------//
//-------Добавление страны--------------//
function AddCountry() {
    $.ajax({
        url: "/Home/AddCountry/",
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalContent').html(result);
            $('#ServicesModal').modal('show');
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }

    })
}

//-------Сохранение добавления страны----------//
function CountrySave() {

    var isValid = true;
    if ($('#Country').val() == "") {
        $('#Country').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#Country').css('border-color', 'lightgrey');
    }

    if (isValid == false) {
        return false;
    }

    var data = {
        //'ID': ID,
        'Login': $('#Country').val(),
        'UserModific': $('#UserModif').val(),

    };

    $.ajax({
        url: "/Home/CountrySave/",
        type: "POST",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(data),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalContent').html(result);
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });

}
//--------------------------------//
// Удаление страны//

function DeleteCountry(ID) {
    $.ajax({
        url: "/Home/DeleteCountry/" + ID,
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalDeleteContent').html(result);
            $('#ServicesModalDelete').modal('show');
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });
}
//-----------------------------//
//Подтверждение удаления страны//
function DeleteCountryOK(ID) {


    $.ajax({
        url: "/Home/DeleteCountryOK/" + ID,
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalDeleteContent').html(result);
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });
}
//-----------------------------//
// Редактирование страны//

function CountryEdit(ID) {
    $.ajax({
        url: "/Home/CountryEdit/" + ID,
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalContent').html(result);
            $('#ServicesModal').modal('show');
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }

    })
}
//-------------------------//
// Сохранение редактирования страны//

function CountryEditSave() {

    var isValid = true;

    if ($('#CountryEdit').val() == "") {
        $('#CountryEdit').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#CountryEdit').css('border-color', 'lightgrey');
    }

    if (isValid == false) {
        return false;
    }

    var data = {

        'Id': $('#IDEdit').val(),
        'Login': $('#CountryEdit').val(),
        'UserModific': $('#UserModifEdit').val(),

    };

    $.ajax({
        url: "/Home/CountryEditSave/",
        type: "POST",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(data),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalContent').html(result);
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });
}
//------------------------------------------------------------//

//---------------САНАТОРИИ-------------------------------------//
//-------Добавление санатория--------------//
function AddSanatorium() {
    $.ajax({
        url: "/Home/AddSanatorium/",
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalContent').html(result);
            $('#ServicesModal').modal('show');
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }

    })
}

//-------Сохранение добавления санатория----------//
function SanatoriumSave() {

    var isValid = true;

    if ($('#Sanatorium').val() == "") {
        $('#Sanatorium').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#Sanatorium').css('border-color', 'lightgrey');
    }


    if ($('#idUS').val() == "0") {
        $('#idUS').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#idUS').css('border-color', 'lightgrey');
    }


    if ($('#BankID').val() == "0") {
        $('#BankID').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#BankID').css('border-color', 'lightgrey');
    }


    if ($('#Address').val() == "") {
        $('#Address').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#Address').css('border-color', 'lightgrey');
    }


    if ($('#PostAddress').val() == "") {
        $('#PostAddress').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#PostAddress').css('border-color', 'lightgrey');
    }
   
    
    if ($('#UNP').val() == "") {
        $('#UNP').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#UNP').css('border-color', 'lightgrey');
    }

    if ($('#RS').val() == "") {
        $('#RS').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#RS').css('border-color', 'lightgrey');
    }

    if ($('#Kontr').val() == "0") {
        $('#Kontr').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#Kontr').css('border-color', 'lightgrey');
    }

    if (isValid == false) {
        return false;
    }

    var data = {
        //'ID': ID,
        'Name': $('#Sanatorium').val(),
        'CityId': $('#idUS').val(),
        'Address': $('#Address').val(),
        'PostAddress': $('#PostAddress').val(),
        'Unp': $('#UNP').val(),
        'BankID': $('#BankID').val(),
        'UserMod': $('#UserModif').val(),
        'SanatInd': $('#RS').val(),
        'Priznak': $('#Kontr').val(),

    };

    $.ajax({
        url: "/Home/SanatoriumSave/",
        type: "POST",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(data),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalContent').html(result);
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });

}
//--------------------------------//
// Удаление санатория//

function DeleteSanatorium(ID) {
    $.ajax({
        url: "/Home/DeleteSanatorium/" + ID,
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalDeleteContent').html(result);
            $('#ServicesModalDelete').modal('show');
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });
}
//-----------------------------//
//Подтверждение удаления санатория//
function DeleteSanatoriumOK(ID) {


    $.ajax({
        url: "/Home/DeleteSanatoriumOK/" + ID,
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalDeleteContent').html(result);
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });
}
//-----------------------------//
// Редактирование санатория//

function SanatoriumEdit(ID) {
    $.ajax({
        url: "/Home/SanatoriumEdit/" + ID,
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalContent').html(result);
            $('#ServicesModal').modal('show');
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }

    })
}
//-------------------------//
// Сохранение редактирования санатория//

function SanatoriumEditSave() {

    var isValid = true;

    if ($('#SanatoriumEdit').val() == "") {
        $('#SanatoriumEdit').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#SanatoriumEdit').css('border-color', 'lightgrey');
    }

    if ($('#RSEdit').val() == "") {
        $('#RSEdit').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#RSEdit').css('border-color', 'lightgrey');
    }

    if (isValid == false) {
        return false;
    }

    var data = {

        'Id': $('#IDEdit').val(),
        'Name': $('#SanatoriumEdit').val(),
        'CityId': $('#idUSEdit').val(),
        'Address': $('#AddressEdit').val(),
        'PostAddress': $('#PostAddressEdit').val(),
        'Unp': $('#UNPEdit').val(),
        'BankID': $('#BankIDEdit').val(),
        'SanatInd': $('#RSEdit').val(),
        'Priznak': $('#KontrEdit').val(),
        'UserMod': $('#UserModifEdit').val(),

    };

    $.ajax({
        url: "/Home/SanatoriumEditSave/",
        type: "POST",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(data),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalContent').html(result);
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });
}
//------------------------------------------------------------//

//---------------СОТРУДНИКИ-------------------------------------//
//-------Добавление сотрудника--------------//
function AddEmployee() {
    $.ajax({
        url: "/Home/AddEmployee/",
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalContent').html(result);
            $('#ServicesModal').modal('show');
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }

    })
}

//-------Сохранение добавления сотрудника----------//
function EmployeeSave() {

    var isValid = true;

    if ($('#First').val() == "") {
        $('#First').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#First').css('border-color', 'lightgrey');
    }

    if ($('#Last').val() == "") {
        $('#Last').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#Last').css('border-color', 'lightgrey');
    }

    if ($('#Middle').val() == "") {
        $('#Middle').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#Middle').css('border-color', 'lightgrey');
    }

    if ($('#Address').val() == "") {
        $('#Address').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#Address').css('border-color', 'lightgrey');
    }

    if ($('#Pol').val() == "0") {
        $('#Pol').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#Pol').css('border-color', 'lightgrey');
    }

    if ($('#Fillial').val() == "0") {
        $('#Fillial').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#Fillial').css('border-color', 'lightgrey');
    }

    if ($('#Department').val() == "0") {
        $('#Department').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#Department').css('border-color', 'lightgrey');
    }
    if ($('#Position').val() == "0") {
        $('#Position').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#Position').css('border-color', 'lightgrey');
    }

    if ($('#BirthDay').val() == "") {
        $('#BirthDay').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#BirthDay').css('border-color', 'lightgrey');
    }

    if (isValid == false) {
        return false;
    }

    var data = {
        //'ID': ID,
        'FirstName': $('#First').val(),
        'LastName': $('#Last').val(),
        'MiddleName': $('#Middle').val(),
        'PositionId': $('#Position').val(),
        'DepartmentId': $('#Department').val(),
        'FilialId': $('#Fillial').val(),
        'Address': $('#Address').val(),
        'Pol': $('#Pol').val(),
        'Pol': $('#Pol').val(),
        'DateBirth': $('#BirthDay').val(),
        'UserModific': $('#UserModif').val(),

    };

    $.ajax({
        url: "/Home/EmployeeSave/",
        type: "POST",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(data),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalContent').html(result);
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });

}
//--------------------------------//
// Удаление сотрудника//

function DeleteEmployee(ID) {
    $.ajax({
        url: "/Home/DeleteEmployee/" + ID,
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalDeleteContent').html(result);
            $('#ServicesModalDelete').modal('show');
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });
}
//-----------------------------//
//Подтверждение удаления сотрудника//
function DeleteEmployeeOK(ID) {


    $.ajax({
        url: "/Home/DeleteEmployeeOK/" + ID,
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalDeleteContent').html(result);
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });
}
//-----------------------------//
// Редактирование сотрудника//

function EmployeeEdit(ID) {
    $.ajax({
        url: "/Home/EmployeeEdit/" + ID,
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalContent').html(result);
            $('#ServicesModal').modal('show');
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }

    })
}
//-------------------------//
// Сохранение редактирования сотрудника//

function EmployeeEditSave() {

    var isValid = true;

    if ($('#FirstEdit').val() == "") {
        $('#FirstEdit').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#FirstEdit').css('border-color', 'lightgrey');
    }

    if ($('#LastEdit').val() == "") {
        $('#LastEdit').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#LastEdit').css('border-color', 'lightgrey');
    }

    if ($('#MiddleEdit').val() == "") {
        $('#MiddleEdit').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#MiddleEdit').css('border-color', 'lightgrey');
    }

    if ($('#AddressEdit').val() == "") {
        $('#AddressEdit').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#AddressEdit').css('border-color', 'lightgrey');
    }

    if ($('#PolEdit').val() == "") {
        $('#PolEdit').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#PolEdit').css('border-color', 'lightgrey');
    }

    if (isValid == false) {
        return false;
    }

    var data = {

        'Id': $('#IDEdit').val(),
        'FirstName': $('#FirstEdit').val(),
        'LastName': $('#LastEdit').val(),
        'MiddleName': $('#MiddleEdit').val(),
        'PositionId': $('#PositionEdit').val(),
        'DepartmentId': $('#DepartmentEdit').val(),
        'FilialId': $('#FillialEdit').val(),
        'Address': $('#AddressEdit').val(),
        'Pol': $('#PolEdit').val(),
        'DateBirth': $('#BithDayEdit').val(),
        'UserModific': $('#UserModif').val(),
        'Fiorp': $('#RPEdit').val(),
        'Fiodp': $('#DPEdit').val(),
        'Fiovp': $('#VPEdit').val(),
        'Fiotp': $('#TPEdit').val(),
        'Fiopp': $('#PPEdit').val(),

    };

    $.ajax({
        url: "/Home/EmployeeEditSave/",
        type: "POST",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(data),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalContent').html(result);
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });
}
//------------------------------------------------------------//

//---------------ДЕТИ-------------------------------------//
//-------Добавление ребенка--------------//
function AddChild() {
    $.ajax({
        url: "/Home/AddChild/",
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalContent').html(result);
            $('#ServicesModal').modal('show');
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }

    })
}

//-------Сохранение добавления ребенка----------//
function ChildSave() {

    var isValid = true;

    if ($('#FIO').val() == "") {
        $('#FIO').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#FIO').css('border-color', 'lightgrey');
    }

    if ($('#EmployeeID').val() == "") {
        $('#EmployeeID').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#EmployeeID').css('border-color', 'lightgrey');
    }

    if ($('#StatusID').val() == "") {
        $('#StatusID').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#StatusID').css('border-color', 'lightgrey');
    }

    if ($('#Pol').val() == "") {
        $('#Pol').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#Pol').css('border-color', 'lightgrey');
    }

    if ($('#BirthDay').val() == "") {
        $('#BirthDay').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#BirthDay').css('border-color', 'lightgrey');
    }


    if (isValid == false) {
        return false;
    }

    var data = {
        'FIO': $('#FIO').val(),
        'EmployeeID': $('#EmployeeID').val(),
        'StatusID': $('#StatusID').val(),
        'Pol': $('#Pol').val(),
        'DateBirth': $('#BirthDay').val(),
        'UserModific': $('#UserModif').val(),

    };

    $.ajax({
        url: "/Home/ChildSave/",
        type: "POST",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(data),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalContent').html(result);
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });

}
//--------------------------------//
// Удаление ребенка//

function DeleteChild(ID) {
    $.ajax({
        url: "/Home/DeleteChild/" + ID,
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalDeleteContent').html(result);
            $('#ServicesModalDelete').modal('show');
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });
}
//-----------------------------//
//Подтверждение удаления ребенка//
function DeleteChildOK(ID) {


    $.ajax({
        url: "/Home/DeleteChildOK/" + ID,
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalDeleteContent').html(result);
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });
}
//-----------------------------//
// Редактирование ребенка//

function ChildEdit(ID) {
    $.ajax({
        url: "/Home/ChildEdit/" + ID,
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalContent').html(result);
            $('#ServicesModal').modal('show');
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }

    })
}
//-------------------------//
// Сохранение редактирования ребенка//

function ChildEditSave() {

    var isValid = true;

    if ($('#FIOEdit').val() == "") {
        $('#FIOEdit').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#FIOEdit').css('border-color', 'lightgrey');
    }

    if ($('#EmployeeIDEdit').val() == "") {
        $('#EmployeeIDEdit').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#EmployeeIDEdit').css('border-color', 'lightgrey');
    }

    if ($('#StatusIDEdit').val() == "") {
        $('#StatusIDEdit').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#StatusIDEdit').css('border-color', 'lightgrey');
    }

    if ($('#PolEdit').val() == "") {
        $('#PolEdit').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#PolEdit').css('border-color', 'lightgrey');
    }

    if ($('#BirthDayEdit').val() == "") {
        $('#BirthDayEdit').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#BirthDayEdit').css('border-color', 'lightgrey');
    }

    if (isValid == false) {
        return false;
    }

    var data = {

        'Id': $('#IDEdit').val(),
        'FIO': $('#FIOEdit').val(),
        'EmployeeID': $('#EmployeeIDEdit').val(),
        'StatusID': $('#StatusIDEdit').val(),
        'Pol': $('#PolEdit').val(),
        'DateBirth': $('#BithDayEdit').val(),
        'UserModific': $('#UserModif').val(),
        'UserModific': $('#UserModif').val(),

    };

    $.ajax({
        url: "/Home/ChildEditSave/",
        type: "POST",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(data),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalContent').html(result);
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });
}
//------------------------------------------------------------//

//---------------ПРОТОКОЛЫ-------------------------------------//
//-------Добавление протокола--------------//
function AddProtocol() {
    $.ajax({
        url: "/Home/AddProtocol/",
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalContent').html(result);
            $('#ServicesModal').modal('show');
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }

    })
}

//-------Сохранение добавления протокола----------//
function ProtocolSave() {

    var isValid = true;

    if ($('#Number').val() == "") {
        $('#Number').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#Number').css('border-color', 'lightgrey');
    }

    if ($('#DateP').val() == "0") {
        $('#DateP').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#DateP').css('border-color', 'lightgrey');
    }

    if (isValid == false) {
        return false;
    }

    var data = {
        //'ID': ID,
        'Login': $('#Number').val(),
        'DateMod': $('#DateP').val(),
        'EmployeeID': $('#Filial').val(),
        'UserModific': $('#UserModif').val(),

    };

    $.ajax({
        url: "/Home/ProtocolSave/",
        type: "POST",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(data),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalContent').html(result);
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });

}
//--------------------------------//
// Удаление протокола//

function DeleteProtocol(ID) {
    $.ajax({
        url: "/Home/DeleteProtocol/" + ID,
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalDeleteContent').html(result);
            $('#ServicesModalDelete').modal('show');
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });
}
//-----------------------------//
//Подтверждение удаления протокола//
function DeleteProtocolOK(ID) {


    $.ajax({
        url: "/Home/DeleteProtocolOK/" + ID,
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalDeleteContent').html(result);
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });
}
//-----------------------------//
// Редактирование протокола//

function ProtocolEdit(ID) {
    $.ajax({
        url: "/Home/ProtocolEdit/" + ID,
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalContent').html(result);
            $('#ServicesModal').modal('show');
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }

    })
}
//-------------------------//
// Сохранение редактирования протокола//

function ProtocolEditSave() {

    var isValid = true;

    if ($('#NumberEdit').val() == "") {
        $('#NumberEdit').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#NumberEdit').css('border-color', 'lightgrey');
    }

    if ($('#DatePEdit').val() == "") {
        $('#DatePEdit').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#DatePEdit').css('border-color', 'lightgrey');
    }


    if (isValid == false) {
        return false;
    }

    var data = {

        'Id': $('#IDEdit').val(),
        'Login': $('#NumberEdit').val(),
        'DateMod': $('#DatePEdit').val(),
        'UserModific': $('#UserModifEdit').val(),

    };

    $.ajax({
        url: "/Home/ProtocolEditSave/",
        type: "POST",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(data),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalContent').html(result);
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });
}
//------------------------------------------------------------//

//---------------ЗАЯВЛЕНИЯ-------------------------------------//
//-------Добавление заявления--------------//
function AddZay() {
    $.ajax({
        url: "/Home/AddZay/",
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalContent').html(result);
            $('#ServicesModal').modal('show');
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }

    })
}

//-------Сохранение добавления заявления----------//
function ZaySave() {

    var isValid = true;

    if ($('#number').val() == "") {
        $('#number').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#number').css('border-color', 'lightgrey');
    }

    if ($('#dataz').val() == "") {
        $('#dataz').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#dataz').css('border-color', 'lightgrey');
    }

    if ($('#prot').val() == "-1") {
        $('#prot').css('border-color', 'Red');
        isValid = false;       
    }
    else {
        $('#prot').css('border-color', 'lightgrey');
    }

    if ($('#sotr').val() == "0") {
        $('#sotr').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#sotr').css('border-color', 'lightgrey');
    }

    if ($('#Sanatorium').val() == "0") {
        $('#Sanatorium').css('border-color', 'Red');
        isValid = false;        
    }
    else {
        $('#Sanatorium').css('border-color', 'lightgrey');
    }
console.log("prot= " + $('#prot').val());
    if ($('#datas').val() == "") {
        $('#datas').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#datas').css('border-color', 'lightgrey');
    }

    if ($('#datapo').val() == "") {
        $('#datapo').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#datapo').css('border-color', 'lightgrey');
    }

    if (isValid == false) {
        return false;
    }
       
    //-------------------------------------------

    var col = $('input[type="radio"]:checked').attr('id');
    {
        if (col == '1') {
            kogo = 'работника';
        }
        if (col == '4') {
            kogo = 'пенсионера';
        }
        if (col == '2') {
            kogo = 'детей';
        }
        if (col == '3') {
            kogo = 'семейная';

        }

    }

    //-----------------передача выбранных checkbox-----------------------

    var selected = []
    var checkboxes = document.querySelectorAll('input[name=inList]:checked');

    for (var i = 0; i < checkboxes.length; i++) {
        selected.push(checkboxes[i].value);
    }
    //var deti []
    //deti = selected.push(checkboxes[i].value);


    //-----------------------------------------------


    var data = {

        'NumberZ': $('#number').val(),
        'DateZ': $('#dataz').val(),
        'EmployeeId': $('#sotr').val(),
        'S': $('#datas').val(),
        'Po': $('#datapo').val(),
        'ProtocolId': $('#prot').val(),
        'Who': kogo,
        'Summa': ($('#Summa').val()).replace(",", "."),
        'SummaDop': ($('#SummaDop').val()).replace(",", "."),
        'SanatoriumId': $('#Sanatorium').val(),
        'UserMod': $('#UserModif').val(),
        'TableZ': selected
    };

    $.ajax({
        url: "/Home/ZaySave/",
        type: "POST",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(data),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalContent').html(result);
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });

}
//--------------------------------//
// Удаление Заявления//

function DeleteZay(ID) {
    $.ajax({
        url: "/Home/DeleteZay/" + ID,
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalDeleteContent').html(result);
            $('#ServicesModalDelete').modal('show');
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });
}
//-----------------------------//
//Подтверждение удаления заявления//
function DeleteZayOK(ID) {


    $.ajax({
        url: "/Home/DeleteZayOK/" + ID,
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalDeleteContent').html(result);
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });
}
//-----------------------------//
// Редактирование заявления//

function ZayEdit(ID) {
    $.ajax({
        url: "/Home/ZayEdit/" + ID,
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalContent').html(result);
            $('#ServicesModal').modal('show');            
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }

    })
}
//-------------------------//
// Сохранение редактирования заявления//

function ZayEditSave() {

    var isValid = true;

    if ($('#numberEdit').val() == "") {
        $('#numberEdit').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#numberEdit').css('border-color', 'lightgrey');
    }

    if ($('#datazEdit').val() == "") {
        $('#datazEdit').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#datazEdit').css('border-color', 'lightgrey');
    }

    if ($('#protEdit').val() == "0") {
        $('#protEdit').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#protEdit').css('border-color', 'lightgrey');
    }

    if ($('#sotrEdit').val() == "0") {
        $('#sotrEdit').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#sotrEdit').css('border-color', 'lightgrey');
    }

    if ($('#SanatoriumEdit').val() == "0") {
        $('#SanatoriumEdit').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#SanatoriumEdit').css('border-color', 'lightgrey');
    }

    if ($('#datasEdit').val() == "") {
        $('#datasEdit').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#datasEdit').css('border-color', 'lightgrey');
    }

    if ($('#datapoEdit').val() == "") {
        $('#datapoEdit').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#datapoEdit').css('border-color', 'lightgrey');
    }

    var col = $('input[type="radio"]:checked').attr('id');
    {
        if (col == '1') {
            kogo = 'работника';
        }
        if (col == '4') {
            kogo = 'пенсионера';
        }
        if (col == '2') {
            kogo = 'детей';
        }
        if (col == '3') {
            kogo = 'семейная';

        }

    }

    if (isValid == false) {
        return false;
    }

    //-----------------передача выбранных checkbox-----------------------

    var selected = []
    var checkboxes = document.querySelectorAll('input[name=inList]:checked');

    for (var i = 0; i < checkboxes.length; i++) {
        selected.push(checkboxes[i].value);
    }
    //var deti []
    //deti = selected.push(checkboxes[i].value);
    var sum = 0;
    if ($('#SummaEdit').val() == "")
    {
        sum = 0;
    }
    else {
        sum = ($('#SummaEdit').val()).replace(",", ".")
    }
    var sumD = 0;
    if ($('#SummaDopEdit').val() == "") {
        sumD = 0;
    }
    else {
        sumD = ($('#SummaDopEdit').val()).replace(",", ".")
    }
    //-----------------------------------------------

    //------Проверяем оплачена путевка или нет--------------------
    var chek2;
    var col = $('input[type="radio"]:checked').attr('id');
    {
        if (col == '0') {
            chek2 = '0';
        }
        if (col == '1') {
            chek2 = '1';
        }

        //--------------------------------------------------------
        //------------------------------------------------
        if (rulesOplata.checked) { chek2 = '1'; }
        else { chek2 = '0'; }
        //-----------------------------------------------
    }
    //------Проверяем отчитались или нет--------------------
    var chek3;
    var col3 = $('input[type="radio"]:checked').attr('id');
    {
        if (col3 == '0') {
            chek3 = '0';
        }
        if (col3 == '1') {
            chek3 = '1';
        }

        //--------------------------------------------------------
        //------------------------------------------------
        if (rulesClose.checked) { chek3 = 'принято'; }
        else { chek3 = 'не принято'; }
        //-----------------------------------------------
    }
    //------Проверяем анулирована путевка или нет--------------------
    var chek4;
    var col4 = $('input[type="radio"]:checked').attr('id');
    {
        if (col4 == '0') {
            chek4 = '0';
        }
        if (col4 == '1') {
            chek4 = '1';
        }

        //--------------------------------------------------------
        //------------------------------------------------
        if (rulesNule.checked) { chek4 = '1'; }
        else { chek4 = '0'; }
        //-----------------------------------------------
    }

    var data = {

        'Id': $('#IDEdit').val(),
        'NumberZ': $('#numberEdit').val(),
        'DateZ': $('#datazEdit').val(),
        'EmployeeId': $('#sotrEdit').val(),
        'S': $('#datasEdit').val(),
        'Po': $('#datapoEdit').val(),
        'ProtocolId': $('#protocolEdit').val(),
        'Summa': sum,
        'SummaDop': sumD,
        'Who': kogo,
        'PriznakOplata': chek2,
        'Priznak': chek3,
        'Anulirovano': chek4,
        'SanatoriumId': $('#placeEdit').val(),
        'TurOpeId': $('#KontrEdit').val(),
        'UserMod': $('#UserModifEdit').val(),
        'TableZ': selected

    };

    $.ajax({
        url: "/Home/ZayEditSave/",
        type: "POST",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(data),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalContent').html(result);
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });
}
//----------Работаем с разовым заявлением--------------------------------------------
//-------Добавление заявления--------------//
function AddZay1() {
    $.ajax({
        url: "/Home/AddZay1/",
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalContent').html(result);
            $('#ServicesModal').modal('show');
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }

    })
}

//-------Сохранение добавления заявления----------//
function ZaySave1() {

    var isValid = true;

    if ($('#number').val() == "") {
        $('#number').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#number').css('border-color', 'lightgrey');
    }

    if ($('#dataz').val() == "") {
        $('#dataz').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#dataz').css('border-color', 'lightgrey');
    }

    if ($('#prot').val() == "-1") {
        $('#prot').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#prot').css('border-color', 'lightgrey');
    }

    if ($('#sotr').val() == "0") {
        $('#sotr').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#sotr').css('border-color', 'lightgrey');
    }

    if ($('#Sanatorium').val() == "0") {
        $('#Sanatorium').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#Sanatorium').css('border-color', 'lightgrey');
    }

    if ($('#Tur').val() == "0") {
        $('#Tur').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#Tur').css('border-color', 'lightgrey');
    }

    if ($('#NumberDog').val() == "") {
        $('#NumberDog').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#NumberDog').css('border-color', 'lightgrey');
    }

    if ($('#DateDog').val() == "") {
        $('#DateDog').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#DateDog').css('border-color', 'lightgrey');
    }

    console.log("prot= " + $('#prot').val());
    if ($('#datas').val() == "") {
        $('#datas').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#datas').css('border-color', 'lightgrey');
    }

    if ($('#datapo').val() == "") {
        $('#datapo').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#datapo').css('border-color', 'lightgrey');
    }

    if (isValid == false) {
        return false;
    }

    //-------------------------------------------

    var col = $('input[type="radio"]:checked').attr('id');
    {
        if (col == '1') {
            kogo = 'работника';
        }
        if (col == '4') {
            kogo = 'пенсионера';
        }
        if (col == '2') {
            kogo = 'детей';
        }
        if (col == '3') {
            kogo = 'семейная';

        }

    }

    //-----------------передача выбранных checkbox-----------------------

    var selected = []
    var checkboxes = document.querySelectorAll('input[name=inList]:checked');

    for (var i = 0; i < checkboxes.length; i++) {
        selected.push(checkboxes[i].value);
    }
    //var deti []
    //deti = selected.push(checkboxes[i].value);


    //-----------------------------------------------


    var data = {

        'NumberZ': $('#number').val(),
        'DateZ': $('#dataz').val(),
        'EmployeeId': $('#sotr').val(),
        'S': $('#datas').val(),
        'Po': $('#datapo').val(),
        'ProtocolId': $('#prot').val(),
        'TurOpeId': $('#Tur').val(),
        'NumberDog': $('#NumberDog').val(),
        'DateDog': $('#DateDog').val(),
        'Who': kogo,
        'Summa': ($('#Summa').val()).replace(",", "."),
        'SummaDop': ($('#SummaDop').val()).replace(",", "."),
        'SanatoriumId': $('#Sanatorium').val(),
        'UserMod': $('#UserModif').val(),
        'TableZ': selected
    };

    $.ajax({
        url: "/Home/ZaySave1/",
        type: "POST",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(data),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalContent').html(result);
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });

}
//--------------------------------//
// Редактирование заявления//

function ZayEdit1(ID) {
    $.ajax({
        url: "/Home/ZayEdit1/" + ID,
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalContent').html(result);
            $('#ServicesModal').modal('show');
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }

    })
}
//-------------------------//
// Сохранение редактирования заявления//

function ZayEditSave1() {

    var isValid = true;

    if ($('#numberEdit').val() == "") {
        $('#numberEdit').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#numberEdit').css('border-color', 'lightgrey');
    }

    if ($('#datazEdit').val() == "") {
        $('#datazEdit').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#datazEdit').css('border-color', 'lightgrey');
    }

    if ($('#protEdit').val() == "0") {
        $('#protEdit').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#protEdit').css('border-color', 'lightgrey');
    }

    if ($('#sotrEdit').val() == "0") {
        $('#sotrEdit').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#sotrEdit').css('border-color', 'lightgrey');
    }

    if ($('#SanatoriumEdit').val() == "0") {
        $('#SanatoriumEdit').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#SanatoriumEdit').css('border-color', 'lightgrey');
    }

    if ($('#datasEdit').val() == "") {
        $('#datasEdit').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#datasEdit').css('border-color', 'lightgrey');
    }

    if ($('#datapoEdit').val() == "") {
        $('#datapoEdit').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#datapoEdit').css('border-color', 'lightgrey');
    }

    if ($('#KontrEdit').val() == "0") {
        $('#KontrEdit').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#KontrEdit').css('border-color', 'lightgrey');
    }

    if ($('#NumberDogEdit').val() == "0") {
        $('#NumberDogEdit').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#NumberDogEdit').css('border-color', 'lightgrey');
    }

    if ($('#DateDogEdit').val() == "0") {
        $('#DateDogEdit').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#DateDogEdit').css('border-color', 'lightgrey');
    }

    var col = $('input[type="radio"]:checked').attr('id');
    {
        if (col == '1') {
            kogo = 'работника';
        }
        if (col == '4') {
            kogo = 'пенсионера';
        }
        if (col == '2') {
            kogo = 'детей';
        }
        if (col == '3') {
            kogo = 'семейная';
        }
    }

    if (isValid == false) {
        return false;
    }

    //-----------------передача выбранных checkbox-----------------------

    var selected = []
    var checkboxes = document.querySelectorAll('input[name=inList]:checked');

    for (var i = 0; i < checkboxes.length; i++) {
        selected.push(checkboxes[i].value);
    }
    //var deti []
    //deti = selected.push(checkboxes[i].value);
    var sum = 0;
    if ($('#SummaEdit').val() == "") {
        sum = 0;
    }
    else {
        sum = ($('#SummaEdit').val()).replace(",", ".")
    }
    var sumD = 0;
    if ($('#SummaDopEdit').val() == "") {
        sumD = 0;
    }
    else {
        sumD = ($('#SummaDopEdit').val()).replace(",", ".")
    }
    //-----------------------------------------------
    var data = {

        'Id': $('#IDEdit').val(),
        'NumberZ': $('#numberEdit').val(),
        'DateZ': $('#datazEdit').val(),
        'EmployeeId': $('#sotrEdit').val(),
        'S': $('#datasEdit').val(),
        'Po': $('#datapoEdit').val(),
        'ProtocolId': $('#protocolEdit').val(),
        'Summa': sum,
        'SummaDop': sumD,
        'TurOpeId': $('#KontrEdit').val(),
        'NumberDog': $('#NumberDogEdit').val(),
        'DateDog': $('#DateDogEdit').val(),
        'Who': kogo,
        'SanatoriumId': $('#placeEdit').val(),
        'UserMod': $('#UserModifEdit').val(),
        'TableZ': selected
    };

    $.ajax({
        url: "/Home/ZayEditSave/",
        type: "POST",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(data),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalContent').html(result);
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });
}

//--------------------------------------------------------
//--------------ЗАКРЫТИЕ ЗАЯВЛЕНИЯ----------------------------------------------//

function PutZay(ID) {

    $.ajax({
        url: "/Home/PutZay/" + ID,
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalContent').html(result);
            $('#ServicesModal').modal('show');
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }

    })
}
//------------------------------------------------------------//
//---------------КОМИССИЯ-------------------------------------//
//-------Добавление комиссии--------------//
function AddKomissiya() {
    $.ajax({
        url: "/Home/AddKomissiya/",
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalContent').html(result);
            $('#ServicesModal').modal('show');
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }

    })
}

//-------Сохранение добавления комиссии----------//
function KomissiyaSave() {

    var isValid = true;

    if ($('#status').val() == "-1") {
        $('#status').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#status').css('border-color', 'lightgrey');
    }

    if ($('#Empl').val() == "-1") {
        $('#Empl').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#Empl').css('border-color', 'lightgrey');
    }

    if (isValid == false) {
        return false;
    }

    var data = {
        //'ID': ID,
        'StatusId': $('#status').val(),
        'EmployeeId': $('#Empl').val(),
        'UserModific': $('#UserModif').val(),

    };

    $.ajax({
        url: "/Home/KomissiyaSave/",
        type: "POST",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(data),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalContent').html(result);
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });

}
//--------------------------------//
// Удаление комиссии//

function DeleteKomissiya(ID) {
    $.ajax({
        url: "/Home/DeleteKomissiya/" + ID,
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalDeleteContent').html(result);
            $('#ServicesModalDelete').modal('show');
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });
}
//-----------------------------//
//Подтверждение удаления комиссии//
function DeleteKomissiyaOK(ID) {


    $.ajax({
        url: "/Home/DeleteKomissiyaOK/" + ID,
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalDeleteContent').html(result);
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });
}
//-----------------------------//
// Редактирование комиссии//

function KomissiyaEdit(ID) {
    $.ajax({
        url: "/Home/KomissiyaEdit/" + ID,
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalContent').html(result);
            $('#ServicesModal').modal('show');
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }

    })
}
//-------------------------//
// Сохранение редактирования комиссии//

function KomissiyaEditSave() {

    var isValid = true;

    if ($('#statusEdit').val() == "-1") {
        $('#statusEdit').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#statusEdit').css('border-color', 'lightgrey');
    }

    if ($('#EmplEdit').val() == "-1") {
        $('#EmplEdit').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#EmplEdit').css('border-color', 'lightgrey');
    }

    if (isValid == false) {
        return false;
    }

    //------RadioButton используется или нет----------------------------------------------------
    var chek1;
    var col = $('input[type="radio"]:checked').attr('id');
    {
        if (col == '0') {
            chek1 = '0';
        }
        if (col == '1') {
            chek1 = '1';
        }

        //--------------------------------------------------------
        //------------------------------------------------
        if (rules.checked) { chek1 = '1'; }
        else { chek1 = '0'; }
        //-----------------------------------------------
    }

    var data = {

        'Id': $('#IDEdit').val(),
        'StatusId': $('#statusEdit').val(),
        'EmployeeId': $('#EmplEdit').val(),
        'Priznak': chek1,
        'UserModific': $('#UserModif').val(), 
        'UserModific': $('#UserModifEdit').val(),

    };

    $.ajax({
        url: "/Home/KomissiyaEditSave/",
        type: "POST",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(data),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalContent').html(result);
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });
}
//------------------------------------------------------------//

//---------------Печать протокола----------//
function PrintProtocolWord(ID) {
    window.location = "/Home/Print/" + ID;
}

//---------------Печать заявления----------//
function Report(ID) {
    window.location = "/Home/Report/" + ID;
}

//--------Формирование отчёта-------------------------------------------------------

function ReportView() {

    var RT = document.getElementById('RT');

    var isValid = true;
    if ($('#dataS').val() == "") {
        $('#dataS').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#dataS').css('border-color', 'lightgrey');
    }

    if ($('#dataPO').val() == "") {
        $('#dataPO').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#dataPO').css('border-color', 'lightgrey');
    }
       

    if (isValid == false) {
        return false;
    }

    //-------------------------------------------------------------------------------
    var selected = []
    var checkboxes = document.querySelectorAll('input[name=inList]:checked');

    for (var i = 0; i < checkboxes.length; i++) {
        selected.push(checkboxes[i].value);
    }
    //--------------------------------------------------------------------------------

    var data = {
        //'ID': ID,
        'DateS': $('#dataS').val(),
        'DatePO': $('#dataPO').val(),
        'UserModific': $('#UserModif').val(),
        'Rol': selected

    };

    $.ajax({
        url: "/Home/ReportView",
        type: "POST",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(data),
        dataType: "html",
        success: function (result) {
            $('#TableRep').html(result);

        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });
}
//----------------------------------------------------------------------------------
//---------------Печать отчёта----------//

function ReportDocx() {

    var RT = document.getElementById('RT');

    var isValid = true;
    if ($('#dataS').val() == "") {
        $('#dataS').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#dataS').css('border-color', 'lightgrey');
    }

    if ($('#dataPO').val() == "") {
        $('#dataPO').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#dataPO').css('border-color', 'lightgrey');
    }

    if (isValid == false) {
        return false;
    }

    //-------------------------------------------------------------------------------
    var selected = []
    var checkboxes = document.querySelectorAll('input[name=inList]:checked');

    for (var i = 0; i < checkboxes.length; i++) {
        selected.push(checkboxes[i].value);
    }

    //--------------------------------------------------------------------------------
    var data = {
        'DateS': $('#dataS').val(),
        'DatePO': $('#dataPO').val(),
        'UserModific': $('#UserModif').val(),
        'Rol': selected
    };
    var stringhref = "/Home/ReportDocx?";

    $.ajax({
        url: "/Home/ReportDocx",
        type: "POST",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(data),
        
        success: function () {
            location = '/Report/ReportHealth.docx'
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });
}
//-------Договоры----------------------------------------------------
//-------Добавление договора--------------//
function AddDogovor() {
    $.ajax({
        url: "/Home/AddDogovor/",
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalContent').html(result);
            $('#ServicesModal').modal('show');
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }

    })
}

//-------Сохранение добавления договора----------//
function DogovorSave() {
    
    var isValid = true;
    if ($('#Number').val() == "") {
        $('#Number').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#Number').css('border-color', 'lightgrey');
    }

    if ($('#DataDog').val() == "") {
        $('#DataDog').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#DataDog').css('border-color', 'lightgrey');
    }

    if ($('#DataStart').val() == "") {
        $('#DataStart').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#DataStart').css('border-color', 'lightgrey');
    }


    if ($('#DataEnd').val() == "") {
        $('#DataEnd').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#DataEnd').css('border-color', 'lightgrey');
    }

    if ($('#Sanatorium').val() == "0") {
        $('#Sanatorium').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#Sanatorium').css('border-color', 'lightgrey');
    }

    if ($('#TypeDog').val() == "0") {
        $('#TypeDog').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#TypeDog').css('border-color', 'lightgrey');
    }

    if (isValid == false) {
        return false;
    }

    var data = {
        
        'Number': $('#Number').val(),
        'DateDog': $('#DataDog').val(),
        'DateStart': $('#DataStart').val(),
        'DateEnd': $('#DataEnd').val(),
        'SanatoriumId': $('#Sanatorium').val(),
        'PriznakKontrol': $('#PriznakKontrol').val(),
        'SummaDog': ($('#SummaDog').val()).replace(",", "."),
        'TypeDogId': $('#TypeDog').val(),
        'UserModif': $('#UserModif').val(),        
    };

    $.ajax({
        url: "/Home/DogovorSave/",
        type: "POST",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(data),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalContent').html(result);
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });

}
//--------------------------------//
// Удаление договора//

function DeleteDogovor(ID) {
    $.ajax({
        url: "/Home/DeleteDogovor/" + ID,
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalDeleteContent').html(result);
            $('#ServicesModalDelete').modal('show');
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });
}
//-----------------------------//
//Подтверждение удаления договора//
function DeleteDogovorOK(ID) {


    $.ajax({
        url: "/Home/DeleteDogovorOK/" + ID,
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalDeleteContent').html(result);
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });
}
//-----------------------------//
// Редактирование договора//

function DogovorEdit(ID) {
    $.ajax({
        url: "/Home/DogovorEdit/" + ID,
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalContent').html(result);
            $('#ServicesModal').modal('show');
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }

    })
}
//-------------------------//
// Сохранение редактирования договора//

function DogovorEditSave() {

    var isValid = true;

    if ($('#NumberEdit').val() == "") {
        $('#NumberEdit').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#NumberEdit').css('border-color', 'lightgrey');
    }

    if ($('#DataDogEdit').val() == "") {
        $('#DataDogEdit').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#DataDogEdit').css('border-color', 'lightgrey');
    }

    if ($('#DataStartEdit').val() == "") {
        $('#DataStartEdit').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#DataStartEdit').css('border-color', 'lightgrey');
    }


    if ($('#DataEndEdit').val() == "") {
        $('#DataEndEdit').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#DataEndEdit').css('border-color', 'lightgrey');
    }

    if ($('#SanatoriumEdit').val() == "0") {
        $('#SanatoriumEdit').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#SanatoriumEdit').css('border-color', 'lightgrey');
    }

    if ($('#TypeDogEdit').val() == "0") {
        $('#TypeDogEdit').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#TypeDogEdit').css('border-color', 'lightgrey');
    }
      

    if (isValid == false) {
        return false;
    }

 //------RadioButton используется или нет----------------------------------------------------
    var chek1;
    var col = $('input[type="radio"]:checked').attr('id');
    {
        if (col == '0') {
            chek1 = '0';
        }
        if (col == '1') {
            chek1 = '1';
        }

        //--------------------------------------------------------
        //------------------------------------------------
        if (rules.checked) { chek1 = '1'; }
        else { chek1 = '0'; }
        //-----------------------------------------------
    }

    var data = {
        'Id': $('#IDEdit').val(),
        'Number': $('#NumberEdit').val(),
        'DateDog': $('#DataDogEdit').val(),
        'DateStart': $('#DataStartEdit').val(),
        'DateEnd': $('#DataEndEdit').val(),
        'SanatoriumId': $('#SanatoriumEdit').val(),
        'TypeDogId': $('#TypeDogEdit').val(),
        'PriznakClose': chek1,
        'PriznakKontrol': $('#PriznakKontrolEdit').val(),
        'SummaDog': ($('#SummaDogEdit').val()).replace(",", "."),
        'UserModif': $('#UserModifEdit').val(),

    };

    $.ajax({
        url: "/Home/DogovorEditSave/",
        type: "POST",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(data),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalContent').html(result);
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });
}
//------------------------------------------------------------//
//---------------Печать докладной записки на оплату----------//
function ReportOplata(ID) {
    window.location = "/Home/ReportOplata/" + ID;
}
//-------------------------------------------------------------------

//---------------Печать докладной записки на оплату дополнительных услуг----------//
function ReportDopOplata(ID) {
    window.location = "/Home/ReportDopOplata/" + ID;
}
//-------------------------------------------------------------------




//---------------Банки-------------------------------------//
//-------Добавление банка--------------//
function AddBank() {
    $.ajax({
        url: "/Home/AddBank/",
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalContent').html(result);
            $('#ServicesModal').modal('show');
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }

    })
}

//-------Сохранение добавления банка----------//
function BankSave() {

    var isValid = true;

    if ($('#Name').val() == "") {
        $('#Name').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#Name').css('border-color', 'lightgrey');
    }

    if ($('#idUS').val() == "0") {
        $('#idUS').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#idUS').css('border-color', 'lightgrey');
    }

    if ($('#Address').val() == "") {
        $('#Address').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#Addres').css('border-color', 'lightgrey');
    }
    
    if (isValid == false) {
        return false;
    }

    var data = {
        //'ID': ID,
        'Name': $('#Name').val(),
        'CityId': $('#idUS').val(),
        'Address': $('#Address').val(),
        'Unp': $('#UNP').val(),
        'Okpo': $('#OKPO').val(),
        'Bic': $('#BIC').val(),
        'UserModif': $('#UserModif').val(),

    };

    $.ajax({
        url: "/Home/BankSave/",
        type: "POST",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(data),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalContent').html(result);
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });

}
//--------------------------------//
// Удаление банка//

function DeleteBank(ID) {
    $.ajax({
        url: "/Home/DeleteBank/" + ID,
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalDeleteContent').html(result);
            $('#ServicesModalDelete').modal('show');
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });
}
//-----------------------------//
//Подтверждение удаления банка//
function DeleteBankOK(ID) {


    $.ajax({
        url: "/Home/DeleteBankOK/" + ID,
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalDeleteContent').html(result);
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });
}
//-----------------------------//
// Редактирование банка//

function BankEdit(ID) {
    $.ajax({
        url: "/Home/BankEdit/" + ID,
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalContent').html(result);
            $('#ServicesModal').modal('show');
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }

    })
}
//-------------------------//
// Сохранение редактирования банка//

function BankEditSave() {

    var isValid = true;

    if ($('#NameEdit').val() == "") {
        $('#NameEdit').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#NameEdit').css('border-color', 'lightgrey');
    }

    if ($('#idUSEdit').val() == "0") {
        $('#idUSEdit').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#idUSEdit').css('border-color', 'lightgrey');
    }

    if ($('#AddressEdit').val() == "") {
        $('#AddressEdit').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#AddresEdit').css('border-color', 'lightgrey');
    }
           
    
    if (isValid == false) {
        return false;
    }

    var data = {

        'Id': $('#IDEdit').val(),
        'Name': $('#NameEdit').val(),
        'CityId': $('#idUSEdit').val(),
        'Address': $('#AddressEdit').val(),
        'Unp': $('#UNPEdit').val(),
        'Okpo': $('#OKPOEdit').val(),
        'Bic': $('#BICEdit').val(),
        'UserModif': $('#UserModifEdit').val(),
    };

    $.ajax({
        url: "/Home/BankEditSave/",
        type: "POST",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(data),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalContent').html(result);
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });
}
//------------------------------------------------------------//
//---------------Коэффициенты-------------------------------------//
//-------Добавление коэффициента--------------//
function AddKoef() {
    $.ajax({
        url: "/Home/AddKoef/",
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalContent').html(result);
            $('#ServicesModal').modal('show');
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }

    })
}

//-------Сохранение добавления коэффициента----------//
function KoefSave() {

    var isValid = true;
    if ($('#Name').val() == "") {
        $('#Name').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#Name').css('border-color', 'lightgrey');
    }

    if ($('#DateStart').val() == "") {
        $('#DateStart').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#DateStart').css('border-color', 'lightgrey');
    }

    if ($('#Value').val() == "") {
        $('#Value').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#Value').css('border-color', 'lightgrey');
    }


    if ($('#Control').val() == "") {
        $('#Control').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#Control').css('border-color', 'lightgrey');
    }

    if ($('#Preduprejdenie').val() == "") {
        $('#Preduprejdenie').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#Preduprejdenie').css('border-color', 'lightgrey');
    }

    if (isValid == false) {
        return false;
    }

    var data = {
        //'ID': ID,
        'Name': $('#Name').val(),
        'DateStart': $('#DateStart').val(),
        'Value': ($('#Value').val()).replace(",", "."),
        'Control': ($('#Control').val()).replace(",", "."),
        'Preduptejdenie': ($('#Preduprejdenie').val()).replace(",", "."),
        'Primech': $('#Primech').val(),
        'UserModif': $('#UserModif').val(),
    };

    $.ajax({
        url: "/Home/KoefSave/",
        type: "POST",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(data),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalContent').html(result);
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });

}
//--------------------------------//
// Удаление страны//

function DeleteKoef(ID) {
    $.ajax({
        url: "/Home/DeleteKoef/" + ID,
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalDeleteContent').html(result);
            $('#ServicesModalDelete').modal('show');
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });
}
//-----------------------------//
//Подтверждение удаления страны//
function DeleteKoefOK(ID) {


    $.ajax({
        url: "/Home/DeleteKoefOK/" + ID,
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalDeleteContent').html(result);
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });
}
//-----------------------------//
// Редактирование страны//

function KoefEdit(ID) {
    $.ajax({
        url: "/Home/KoefEdit/" + ID,
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalContent').html(result);
            $('#ServicesModal').modal('show');
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }

    })
}
//-------------------------//
// Сохранение редактирования страны//

function KoefEditSave() {

    var isValid = true;

    if ($('#NameEdit').val() == "") {
        $('#NameEdit').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#NameEdit').css('border-color', 'lightgrey');
    }

    if ($('#DateStartEdit').val() == "") {
        $('#DateStartEdit').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#DateStartEdit').css('border-color', 'lightgrey');
    }

    if ($('#ValueEdit').val() == "") {
        $('#ValueEdit').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#ValueEdit').css('border-color', 'lightgrey');
    }

    if ($('#ControlEdit').val() == "") {
        $('#ControlEdit').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#ControlEdit').css('border-color', 'lightgrey');
    }

    if ($('#PreduprejdenieEdit').val() == "") {
        $('#PreduprejdenieEdit').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#PreduprejdenieEdit').css('border-color', 'lightgrey');
    }

    if (isValid == false) {
        return false;
    }

    var data = {

        'Id': $('#IDEdit').val(),
        'Name': $('#NameEdit').val(),
        'DateStart': $('#DateStartEdit').val(),
        'Value': ($('#ValueEdit').val()).replace(",", "."),
        'Contro': ($('#ControlEdit').val()).replace(",", "."),
        'Preduptejdenie': ($('#PreduprejdenieEdit').val()).replace(",", "."),
        'Primech': $('#PrimechEdit').val(),
        'UserModif': $('#UserModif').val(),

    };

    $.ajax({
        url: "/Home/KoefEditSave/",
        type: "POST",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(data),
        dataType: "html",
        success: function (result) {
            $('#ServicesModalContent').html(result);
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });
}
//------------Получаем список заявлений согласно фильтра------------------------------//
function GetZayTab() {

    var data = {
        'S': $('#SZ').val(),
        'Po': $('#PoZ').val(),
        'Priznak': $('#Prinyat').val(),
        'PriznakOplata': $('#PriznakOplata').val(),
        
    };
    var x = document.getElementById("loadImgZ");
    console.log(x);
    x.style.display = "block";
    $.ajax({
        url: "/Home/GetZayTab/",
        type: "POST",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(data),
        dataType: "html",
        success: function (data) {
            //$('#search').replaceWith(data);
            x.style.display = "none";
            $('#TableZay').hide();
            $('#TableZay').html(data).animate({ opacity: 'show' }, "slow");
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });
}
//------------------------------------------------------------------------------------//
//------------Получаем список протоколов согласно фильтра------------------------------//
function GetProtTab() {

    var data = {
        'S': $('#SZ').val(),
        'Po': $('#PoZ').val(),

    };
    var x = document.getElementById("loadImgZ");
    console.log(x);
    x.style.display = "block";
    $.ajax({
        url: "/Home/GetProtTab/",
        type: "POST",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(data),
        dataType: "html",
        success: function (data) {
            //$('#search').replaceWith(data);
            x.style.display = "none";
            $('#TableProt').hide();
            $('#TableProt').html(data).animate({ opacity: 'show' }, "slow");
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });
}
//Рейтинг санаториев-----
//--------Формирование отчёта-------------------------------------------------------

function ReportViewTop() {

    var RT = document.getElementById('RT');

    var isValid = true;
    if ($('#dataS').val() == "") {
        $('#dataS').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#dataS').css('border-color', 'lightgrey');
    }

    if ($('#dataPO').val() == "") {
        $('#dataPO').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#dataPO').css('border-color', 'lightgrey');
    }


    if (isValid == false) {
        return false;
    }

    //-------------------------------------------------------------------------------
    var selected = []
    var checkboxes = document.querySelectorAll('input[name=inList]:checked');

    for (var i = 0; i < checkboxes.length; i++) {
        selected.push(checkboxes[i].value);
    }
    //--------------------------------------------------------------------------------

    var data = {
        //'ID': ID,
        'DateS': $('#dataS').val(),
        'DatePO': $('#dataPO').val(),
        'UserModific': $('#UserModif').val(),
        'Rol': selected

    };

    $.ajax({
        url: "/Home/ReportViewTop",
        type: "POST",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(data),
        dataType: "html",
        success: function (result) {
            $('#TableRep').html(result);

        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });
}
//---------------Печать списка заявлений----------//

function ReportZayDocx() {

    var RT = document.getElementById('RT');

    var isValid = true;
    if ($('#dataS').val() == "") {
        $('#dataS').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#dataS').css('border-color', 'lightgrey');
    }

    if ($('#dataPO').val() == "") {
        $('#dataPO').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#dataPO').css('border-color', 'lightgrey');
    }

    if (isValid == false) {
        return false;
    }
           
    var data = {
        'S': $('#SZ').val(),
        'Po': $('#PoZ').val(),
        'Priznak': $('#Prinyat').val(),
        'PriznakOplata': $('#PriznakOplata').val(),
    }
    var stringhref = "/Home/ReportZayDocx?";

    $.ajax({
        url: "/Home/ReportZayDocx",
        type: "POST",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify(data),

        success: function () {
            location = '/Report/ReportZayavlenie.docx'
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });
}