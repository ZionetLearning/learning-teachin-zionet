import { BrowserRouter, Route, Routes, Outlet } from "react-router-dom";

import { Box } from "@mui/material";

import { AuthorizationPage, RequireAuth } from "@authorization";
import "./App.css";
import { HomePage } from "./pages";

const ProtectedLayout = () => {
  return (
    <RequireAuth>
      <div data-testid="protected-layout">
        <Box sx={{ display: "flex", height: "100vh" }}>
          <Box sx={{ flexGrow: 1, position: "relative", overflow: "hidden" }}>
            <Outlet />
          </Box>
        </Box>
      </div>
    </RequireAuth>
  );
};

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/signin" element={<AuthorizationPage />} />
        <Route element={<ProtectedLayout />}>
          <Route path="/" element={<HomePage />} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}

export default App;
