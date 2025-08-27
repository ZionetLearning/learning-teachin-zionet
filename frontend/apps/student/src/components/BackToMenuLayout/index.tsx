import { Outlet, useLocation, useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useAuth } from "@app-providers";
import { useStyles } from "./style";

export const BackToMenuLayout = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const classes = useStyles();
  const { logout } = useAuth();
  const { t } = useTranslation();
  const showBackButton = location.pathname !== "/";

  return (
    <div>
      <header className={classes.header}>
        {showBackButton && (
          <button className={classes.button} onClick={() => navigate("/")}>
            {t("goBackToMenu")}
          </button>
        )}
        <button
          className={classes.logoutButton}
          onClick={() => {
            logout();
            navigate("/signin", { replace: true });
          }}
        >
          {t("logout")}
        </button>
      </header>

      <main className={classes.main}>
        <Outlet />
      </main>
    </div>
  );
};
