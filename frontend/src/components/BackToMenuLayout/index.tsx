import { Outlet, useLocation, useNavigate } from "react-router-dom";

import { useAuth } from "@/providers/auth";
import { useStyles } from "./style";

export const BackToMenuLayout = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const classes = useStyles();
  const { logout } = useAuth();

  const showBackButton = location.pathname !== "/";

  return (
    <div>
      <header className={classes.header}>
        {showBackButton && (
          <button className={classes.button} onClick={() => navigate("/")}>
            Go back to menu
          </button>
        )}
        <button
          className={classes.logoutButton}
          onClick={() => {
            logout();
            navigate("/signin", { replace: true });
          }}
        >
          Logout
        </button>
      </header>

      <main className={classes.main}>
        <Outlet />
      </main>
    </div>
  );
};
