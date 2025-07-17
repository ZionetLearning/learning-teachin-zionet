import { useNavigate } from "react-router-dom";

export const HomePage = () => {
  const navigate = useNavigate();

  const handleNavigation = (path: string) => {
    navigate(path);
  };

  return (
    <div
      style={{
        display: "flex",
        flexDirection: "column",
        gap: "16px",
        maxWidth: "600px",
        margin: "0 auto",
        marginTop: "50px",
      }}
    >
      <h1>Welcome to our internal playground project</h1>

      <h2>Chat Tools</h2>
      <button onClick={() => handleNavigation("/chat/yo")}>
        Chat - Yo (Yonatan)
      </button>
      <button onClick={() => handleNavigation("/chat/da")}>
        Chat - Da (Daniel)
      </button>

      <h2>Avatar Tools</h2>
      <button onClick={() => handleNavigation("/avatar/ou")}>
        Avatar - Ou (Ouriel)
      </button>
      <button onClick={() => handleNavigation("/avatar/sh")}>
        Avatar - Sh (Shirley)
      </button>
    </div>
  );
};

