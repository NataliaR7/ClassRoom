import React from "react";
import "../cssDirectory/UserAuthorizationPage.css"
import ReactDOM from "react-dom";
import Page from "./pageProvider";
import {srcUrl} from "../mySettings";
import isEmptyField from "../validation/isEmptyField";
import warnEmptinessHidden from "../validation/warnEmptinessHidden";
import warnEmail from "../validation/warnEmail";
import isEmail from "../validation/isEmail";
import warnPassword from "../validation/warnPassword";
import warnEqualPasswords from "../validation/warnEqualPasswords";
import {useParams} from "react-router-dom"


const formName = "registration";

function isValidForm(): boolean {
    return !isEmptyField(formName, "Surname") && !isEmptyField(formName, "Name")
        && !isEmptyField(formName, "Patronymic") && !isEmptyField(formName, "Username")
        // @ts-ignore
        && isEmail(document.forms[formName]["Email"].value) && document.forms[formName]["PasswordRepeat"].value === document.forms[formName]["Password"].value;
    //email pass;
}


function UserRegistrationPage() {
    let {id} = useParams();

    function sendRegistration() {
        //if (!isValidForm()) {
        // @ts-ignore
        document.querySelector('#registerUserButton').setAttribute("disabled", "true");
        let form: any = document.forms.namedItem(formName);
        let formData = new FormData(form);
        formData.append('GroupId', id);
        fetch(`${srcUrl}/users`,
            {
                method: "post",
                credentials: "include",
                body: formData
            }).then(res => {
            if (res.status === 200)
                window.location.href = window.location.origin + '/main'
        });
        //return false;
        // }
        // let form = document.getElementById("authForm");
    }


    return (
        <div id="userAuthorization">
            <form id="userAuthorizationWindow" name={formName} onSubmit={(e) => {
                e.preventDefault();
                sendRegistration();
            }} /*onChange={() => {
                if (!isValidForm()) {
                    // @ts-ignore
                    document.querySelector('#registerUserButton').setAttribute("disabled", "true")
                } else {
                    // @ts-ignore
                    document.querySelector('#registerUserButton').removeAttribute("disabled")
                }
            }}*/>
                <button id="toGroupForm" onClick={() => {
                    ReactDOM.render(
                        Page.GroupRegistrationPage(),
                        document.getElementById('root')
                    );
                }}/>

                <span id="userAuthorizationWindowHeader">РЕГИСТРАЦИЯ</span>
                <>
                    <input name="GroupId" className="userAuthorizationField" placeholder="Группа" disabled
                           value={id}/>
                    <span className="authorizationContentStatus" id="surnameMessage">Поле не может быть пустым</span>
                    <input name="Surname" className="userAuthorizationField" placeholder="Фамилия" onChange={() => {
                        warnEmptinessHidden(formName, "Surname", "surnameMessage")
                    }}/>
                    <span className="authorizationContentStatus" id="nameMessage">Поле не может быть пустым</span>
                    <input name="Name" className="userAuthorizationField" placeholder="Имя" onChange={() => {
                        warnEmptinessHidden(formName, "Name", "nameMessage")
                    }}/>
                    <span className="authorizationContentStatus" id="patronymicMessage">Поле не может быть пустым</span>
                    <input name="Patronymic" className="userAuthorizationField" placeholder="Отчество" onChange={() => {
                        warnEmptinessHidden(formName, "Patronymic", "patronymicMessage")
                    }}/>
                    <span className="authorizationContentStatus"
                          id="emailMessage">E-mail в формате example@mail.ru</span>
                    <input name="Email" className="userAuthorizationField" placeholder="E-mail" onChange={() => {
                        warnEmail(formName, "Email", "emailMessage")
                    }}/>
                    <span className="authorizationContentStatus" id="loginMessage">Поле не может быть пустым</span>
                    <input name="Username" className="userAuthorizationField" placeholder="Логин" onChange={() => {
                        warnEmptinessHidden(formName, "Username", "loginMessage")
                    }}/>
                    <span className="authorizationContentStatus" id="passwordMessage">Пароль должен быть не менее 6 символов</span>
                    <input name="Password" className="userAuthorizationField" placeholder="Пароль" type="password"
                           onChange={() => {
                               warnPassword(formName, "Password", "passwordMessage");
                               warnEqualPasswords(formName, "Password", "ConfirmPassword", "passwordRepeatMessage")
                           }}/>
                    <span className="authorizationContentStatus"
                          id="passwordRepeatMessage">Пароли должны совпадать</span>
                    <input name="ConfirmPassword" className="userAuthorizationField" placeholder="Пароль"
                           type="password" onChange={() => {
                        warnEqualPasswords(formName, "Password", "ConfirmPassword", "passwordRepeatMessage")
                    }}/>
                </>
                <button id="registerUserButton" type={"submit"} /*onClick={sendRegistration}*/>РЕГИСТРАЦИЯ</button>
            </form>
        </div>
    )
}

export default UserRegistrationPage
