import { useContext } from "react";
import { AuthContextValue, AuthContext } from "@app-providers/context";

export const useAuth = (): AuthContextValue => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used within an AuthProvider");
  }
  return context;
};
