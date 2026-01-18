import {getQuestions} from "@/lib/actions/question-actions";
import QuestionCard from "@/app/questions/QuestionCard";
import QuestionsHeader from "@/app/questions/QuestionsHeader";
import AppPagination from "@/components/AppPagination";
import {QuestionParams} from "@/lib/types";

export default async function QuestionsPage({searchParams}: {searchParams?: Promise<QuestionParams>}) {
    const params = await searchParams;
    const questions = await getQuestions(params?.tag);
    // const {data: questions, error} = await getQuestions(params);

    // if (error) throw error;

    return (
        <>
            <QuestionsHeader total={questions.length} tag={params?.tag} />
            {questions?.map(question => (
                <div key={question.id} className="py-4 not-last:border-b w-full flex">
                    <QuestionCard question={question} key={question.id}/>
                </div>
            ))}
            {/*<AppPagination totalCount={questions?.totalCount ?? 0} />*/}
        </>
    )
}