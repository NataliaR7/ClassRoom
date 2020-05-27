import React, {useEffect} from "react";
import "../cssDirectory/AuthorizationPage.css"
import ReactDOM from "react-dom";
import Page from "./pageProvider";
import {srcUrl} from "../mySettings";


function AuthorizationPage() {
    function sendAuth() {
        // let form = document.getElementById("authForm");
        let form: any = document.forms.namedItem("authForm");
        let formData = new FormData(form);
        console.log("auth");
        fetch(`${srcUrl}/account/login`, {
            method: 'POST',
            credentials: "include",
            headers: {
                // 'Accept': 'multipart/form-data',
                // 'Content-Type': 'multipart/form-data',
            },
            body: formData,
        })
            .then(response => {
                console.log(response);
                if (response.status === 200) {
                    window.location.reload();
                }

            })
    }

    return (
        <div id="authorizationPage">
            <div id="authorizationWindow">
                <span id="authorizationWindowHeader">ВХОД</span>
                <form name={"authForm"}>
                    <input id="loginInput" name="Username" placeholder="Логин"/>
                    <input id="passwordInput" type="password" name="Password" placeholder="Пароль"/>
                    <input id="rememberMe" name="RememberMe" type="checkbox" value={0}/>
                </form>
                <button id="authorizationButton" onClick={sendAuth}>ВОЙТИ</button>
                <span id="registrationLink" onClick={() => {
                    ReactDOM.render(
                        Page.GroupAuthorizationPage(),
                        document.getElementById('root')
                    )
                }}>Зарегистрировать группу</span>
            </div>
        </div>
    )
}

export default AuthorizationPage
