// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.jsHandInput

let handInputRadio = document.getElementById("jsHandInput");//document.querySelector(".js-gallery-button-back");
let autoInputRadio = document.getElementById("jsAutoInput");

let radioButtonHand = document.getElementById("jsHandInput");

//При первой загрузке делаю выделенный радиобаттон ввести вручную и недоступными поля для загрузки с сайта
if (radioButtonHand) {
    radioButtonHand.checked = true;

    let handInputMass = document.getElementsByName("jsHandAutoInput");

    for (var i = 0; i < handInputMass.length; i++) {
        if (handInputMass[i].checked && handInputMass[i].value === "hands") {

            let elemForHide = document.getElementsByClassName("js-hide-for-auto");

            for (let j = 0; j < elemForHide.length; j++) {
                elemForHide[j].disabled = false;
            }

            var elemForShow = document.getElementsByClassName("js-hide-for-hands");

            for (let j = 0; j < elemForShow.length; j++) {
                elemForShow[j].disabled = true;
            }
        }

    }
}

//При изменении на ручной ввод источника для скачивания делаю недоступными поля для скачивания с сайта

if (handInputRadio) {
    handInputRadio.addEventListener("change",
        function (evt) {
          //  evt.preventDefault();
          let handInputMass = document.getElementsByName("jsHandAutoInput");
            

            for (let i = 0; i < handInputMass.length; i++) {
                if (handInputMass[i].checked && handInputMass[i].value === "hands") {

                    let elemForHide = document.getElementsByClassName("js-hide-for-auto");

                    for (var j = 0; j < elemForHide.length; j++) {
                        elemForHide[j].disabled = false;
                    }

                    let elemForShow = document.getElementsByClassName("js-hide-for-hands");
                    
                    for (let j = 0; j < elemForShow.length; j++) {
                        elemForShow[j].disabled = true;
                    }
                }
                
            }
        });
}

//При выборе скачивания источника инструмента с сайта делаю недоступными элементы для ручного ввода
if (autoInputRadio) {
    autoInputRadio.addEventListener("change",
        function (evt) {
            //evt.preventDefault();
            let handInputMass = document.getElementsByName("jsHandAutoInput");
            
            for (let i = 0; i < handInputMass.length; i++) {
                if (handInputMass[i].checked && handInputMass[i].value === "auto") {
            
                    let elemForHide = document.getElementsByClassName("js-hide-for-auto");
            
                    for (let j = 0; j < elemForHide.length; j++) {
                        elemForHide[j].disabled = true;
                    }
            
                    let elemForShow = document.getElementsByClassName("js-hide-for-hands");
                    
                    for (let j = 0; j < elemForShow.length; j++) {
                        elemForShow[j].disabled = false;
                    }
                }
            }
        });
}
//handInputRadio.addEventListener("click", handler);