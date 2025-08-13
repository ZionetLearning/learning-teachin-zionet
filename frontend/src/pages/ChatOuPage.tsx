import { useEffect } from "react";
import { ChatOu } from "../features";
import { initializeSentry } from "../services";

export const ChatOuPage = () => {
  useEffect(() => {
    initializeSentry();
  }, []);

  return <ChatOu />;
};
