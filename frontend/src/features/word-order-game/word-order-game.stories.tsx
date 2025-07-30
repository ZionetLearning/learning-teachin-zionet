import { WordOrderGame } from "./index";
import { Header, Description, Speaker } from "./components";
import type { Meta, StoryObj } from "@storybook/react";

const meta: Meta = {
    title: "Features/WordOrderGame",
    component: WordOrderGame,
};

export default meta;

export const FullGame: StoryObj = {
    render: () => <WordOrderGame />,
};

export const OnlyHeader: StoryObj = {
    render: () => <Header />,
};

export const OnlyDescription: StoryObj = {
    render: () => <Description />,
};

export const SpeakerNormal: StoryObj = {
    render: () => <Speaker onClick={() => alert("Play normal")} />,
};

export const SpeakerSlow: StoryObj = {
    render: () => <Speaker mode="slow" onClick={() => alert("Play slow")} />,
};
