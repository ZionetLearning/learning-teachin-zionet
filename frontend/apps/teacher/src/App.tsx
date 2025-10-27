import { useEffect } from "react";
import { BrowserRouter, Route, Routes } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { SidebarMenuLayout } from "@ui-components";
import { SidebarMenu } from "./components";
import { AuthorizationPage, RequireAuth } from "@authorization";
import "./App.css";
import { HomePage, ProfilePage, StudentPracticeHistoryPage } from "./pages";
import { AppRole } from "@app-providers";

const ProtectedLayout = () => {
  return (
    <RequireAuth allowedRoles={[AppRole.teacher, AppRole.admin]}>
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
        <Route
          path="/signin"
          element={
            <AuthorizationPage
              allowedRoles={[AppRole.teacher, AppRole.admin]}
            />
          }
        />
        <Route element={<ProtectedLayout />}>
          <Route path="/" element={<HomePage />} />
          <Route path="/profile" element={<ProfilePage />} />
          <Route
            path="/student-practice-history"
            element={<StudentPracticeHistoryPage />}
          />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}

export default App;
