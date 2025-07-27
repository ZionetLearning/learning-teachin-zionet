import { Outlet, useNavigate, useLocation } from "react-router-dom";
import { useStyles } from "./style";

export const BackToMenuLayout = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const classes = useStyles();

  const showBackButton = location.pathname !== "/";

  return (
    <div>
      {showBackButton && (
        <header className={classes.header}>
          <button className={classes.button} onClick={() => navigate("/")}>
            Go back to menu
          </button>
        </header>
      )}
      <main className={classes.main}>
        <Outlet />
      </main>
    </div>
  );
};
