from django.urls import path

from . import views

urlpatterns = [
    path("login/", views.login, name="clavi_login"),
    path("signin-oidc/", views.callback, name="clavi_callback"),
    path("logout/", views.logout, name="clavi_logout"),
    path("signout-callback-oidc/", views.signed_out, name="clavi_signed_out"),
    path("me/", views.me, name="clavi_me"),
]
