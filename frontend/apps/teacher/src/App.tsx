import { useEffect } from "react";
import { BrowserRouter, Route, Routes } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { SidebarMenuLayout } from "@ui-components";
import { SidebarMenu } from "./components";
import { AuthorizationPage, RequireAuth } from "@authorization";
import "./App.css";
import { HomePage, ProfilePage } from "./pages";


const ProtectedLayout = () => {
  return (
    <RequireAuth>
      <div data-testid="protected-layout">
        <SidebarMenuLayout sidebarMenu={<SidebarMenu />} />
      </div>
    </RequireAuth>
  );
};

function App() {
  const { i18n } = useTranslation();

  useEffect(() => {
    document.documentElement.lang = i18n.language;
    document.documentElement.dir = i18n.language === "he" ? "rtl" : "ltr";
  }, [i18n.language]);
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/signin" element={<AuthorizationPage />} />
        <Route element={<ProtectedLayout />}>
          <Route path="/" element={<HomePage />} />
          <Route path="/profile" element={<ProfilePage />} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}

export default App;
