<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8" />
    <title>${realm.displayName?:realm.name} • Login</title>
    <meta name="viewport" content="width=device-width, initial-scale=1" />
</head>
<body>
<div class="container">
  <div class="card">
    <h1>HOSPITAL</h1>
    <p class="lead">Acesso seguro ao sistema</p>

    <#if message??>
      <div class="msg">${message.summary}</div>
    </#if>

    <form id="kc-form-login" action="${url.loginAction}" method="post">
      <label for="username">CPF</label>
      <input tabindex="1" id="username" name="username" value="${(login.username!'')?html}" autocomplete="username" placeholder="Somente números" />

      <label for="password">${msg("password")}</label>
      <input tabindex="2" id="password" name="password" type="password" autocomplete="current-password" />

      <button class="primary" type="submit">${msg("doLogIn")}</button>
    </form>

    <#if realm.registrationAllowed>
      <button class="secondary" onclick="location.href='${url.registrationUrl}'">${msg("doRegister")}</button>
    </#if>

    <p class="small">Ao entrar você concorda com nossos termos de uso.</p>
  </div>
</div>
</body>
</html>
