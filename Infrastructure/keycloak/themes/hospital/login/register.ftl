<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8" />
    <title>${realm.displayName?:realm.name} • Cadastro</title>
    <meta name="viewport" content="width=device-width, initial-scale=1" />
</head>
<body>
<div class="container">
  <div class="card">
    <h1>HOSPITAL</h1>
    <p class="lead">Crie sua conta</p>

    <#if message??>
      <div class="msg">${message.summary}</div>
    </#if>

    <form id="kc-register-form" action="${url.registrationAction}" method="post">
      <label for="firstName">${msg("firstName")}</label>
      <input id="firstName" name="firstName" value="${(register.formData.firstName!'')?html}" autocomplete="given-name" />

      <label for="lastName">${msg("lastName")}</label>
      <input id="lastName" name="lastName" value="${(register.formData.lastName!'')?html}" autocomplete="family-name" />

      <label for="email">${msg("email")}</label>
      <input id="email" name="email" value="${(register.formData.email!'')?html}" autocomplete="email" />

      <input id="username" name="username" type="hidden" value="${(register.formData.username!'')?html}" />

      <label for="password">${msg("password")}</label>
      <input id="password" name="password" type="password" autocomplete="new-password" />

      <label for="password-confirm">${msg("passwordConfirm")}</label>
      <input id="password-confirm" name="password-confirm" type="password" autocomplete="new-password" />

      <label for="cpf">CPF</label>
      <input id="cpf" name="attributes.cpf" value="${(register.formData.get('attributes.cpf')!'')?html}" maxlength="11" placeholder="Somente números" pattern="\d{11}" />
      <script>
        (function(){
          const cpf = document.getElementById('cpf');
          const username = document.getElementById('username');
          function onlyDigits(v){ return (v||'').replace(/\D+/g,''); }
          function sync(){ username.value = onlyDigits(cpf.value); }
          cpf.addEventListener('input', sync);
          document.getElementById('kc-register-form').addEventListener('submit', sync);
        })();
      </script>

      <button class="primary" type="submit">${msg("doRegister")}</button>
    </form>

    <button class="secondary" onclick="location.href='${url.loginUrl}'">${msg("backToLogin")}</button>

    <p class="small">Usaremos seu CPF para serviços de saúde.</p>
  </div>
</div>
</body>
</html>
