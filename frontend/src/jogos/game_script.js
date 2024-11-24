const storyText = document.getElementById("story-text");
const orButton = document.querySelector("button[data-choice='or']");
const andButton = document.querySelector("button[data-choice='and']");
const notButton = document.querySelector("button[data-choice='not']");
const svgElement = document.querySelector("svg");

const storyStates = {
    start: "A Aventura dos Três Caminhos",
    start2: "Em uma terra distante, havia um reino chamado Lógicus, que passava por uma grande crise. Uma antiga magia havia coberto o castelo em uma névoa densa, deixando o rei e toda sua corte presos em um sono profundo. Diziam que apenas um aventureiro de mente afiada poderia quebrar o feitiço e libertar o reino.",
    start3: "Nos arredores do reino, um jovem chamado Alex decidiu encarar o desafio. Ele foi avisado de que, ao final de sua jornada, encontraria três portas, e que cada uma representava uma escolha diferente. Para libertar o reino, ele precisaria escolher a porta correta — mas cada uma levaria a um final único e irreversível.",
    start4: `Quando Alex chegou ao final de sua jornada, ele se deparou com as três portas, cada uma com uma placa misteriosa:<br>
            <strong>Porta da Negação (NOT)</strong><br>
            <strong>Porta das Alternativas (OR)</strong><br>
            <strong>Porta da Conjunção (AND)</strong><br>
            Cada porta levaria a um caminho final diferente para libertar o reino.`,

    or: "A Porta das Alternativas (OR)",
    or2: "Ao passar pela Porta das Alternativas, Alex encontrou uma balança mágica com dois itens: um frasco de elixir e um pergaminho antigo. Uma voz misteriosa lhe disse:'Escolha entre uma cura imediata ou um feitiço que dará outra chance ao reino.'",
    or3: "Alex, conhecedor do poder das alternativas, sabia que poderia escolher qualquer uma das opções e ainda assim trazer uma solução ao reino. Ele ponderou e decidiu usar o elixir para despertar o rei, acreditando que o rei teria sabedoria para resolver o resto. Com a ajuda do rei despertado, o feitiço se desfez, e o povo foi libertado.",
    or4: "Alex foi celebrado pela corte como o herói que sempre busca alternativas e confia nas possibilidades.",

    and: "A Porta da Conjunção (AND)",
    and2: "Ao atravessar a Porta da Conjunção, Alex encontrou uma sala com dois pedestais, cada um com um item: uma chave dourada e um cristal brilhante. Uma voz ecoou:'Para quebrar o feitiço, você precisa tanto da chave quanto do cristal.'",
    and3: "Alex entendeu que, para completar sua missão, precisaria dos dois itens. Ele pegou a chave em uma mão e o cristal na outra e as usou juntas na porta final. O feitiço foi quebrado, e todos no castelo despertaram do sono profundo.",
    and4: "Graças ao esforço de Alex de reunir os dois itens, o reino foi salvo, e ele foi celebrado como o herói que uniu forças para alcançar a vitória.",

    not: "A Porta da Negação (NOT)",
    not2: "Alex entrou pela Porta da Negação e foi transportado para uma sala iluminada, onde uma voz ecoava:'Para quebrar o feitiço, você deve negar tudo o que vê.' Alex pensou no que significava negar a situação atual. Ele percebeu que o feitiço de sono eterno só poderia ser quebrado se ele invertesse o estado de quem estava sob o feitiço.",
    not3: "Ele pegou o amuleto que encontrara durante sua jornada e disse:'Eu desejo que o sono profundo se torne o despertar.' Com essa inversão, o feitiço foi quebrado, e a corte despertou de seu sono. O reino celebrou Alex como um herói, e ele aprendeu que às vezes é necessário contrariar as expectativas para alcançar uma solução.",

    end: "A aventura de Alex terminou de três maneiras diferentes, dependendo de sua escolha lógica.",
    end2: "Ao escolher a Porta da Negação, ele quebrou o feitiço ao inverter sua lógica.",
    end3: "Com a Porta das Alternativas, ele encontrou uma solução alternativa.",
    end4: "E com a Porta da Conjunção, ele uniu os poderes de dois itens para alcançar a vitória.",
    end5: "Assim, Alex aprendeu que, em cada situação, a lógica pode nos guiar a diferentes soluções — e que cada escolha tem seu valor próprio."
};


let gameStarted = false;
let currentStep = 0;
let currentChoice = "";
let initialChoice = ""; // Nova variável para armazenar a escolha inicial

// Desabilita os botões inicialmente
orButton.disabled = true;
andButton.disabled = true;
notButton.disabled = true;

function makeChoice(choice) {
    if (!gameStarted && choice === 'start') {
        currentStep += 1;

        if (currentStep === 1) {
            storyText.innerHTML = storyStates.start;
        } else if (currentStep === 2) {
            storyText.innerHTML = storyStates.start2;
        } else if (currentStep === 3) {
            storyText.innerHTML = storyStates.start3;
        } else if (currentStep === 4) {
            storyText.innerHTML = storyStates.start4;
            gameStarted = true;

            if (svgElement) {
                svgElement.style.opacity = '1';
                svgElement.style.transition = 'opacity 0.5s';
                svgElement.style.opacity = '0';

                svgElement.addEventListener('transitionend', () => {
                    svgElement.style.display = 'none';
                    orButton.disabled = false;
                    andButton.disabled = false;
                    notButton.disabled = false;
                }, { once: true });
            }
        }
        return;
    }

    if (gameStarted && !initialChoice) {
        initialChoice = choice;
        currentChoice = choice;
        currentStep = 1;

        if (choice === 'or') {
            storyText.innerHTML = storyStates.or;
        } else if (choice === 'and') {
            storyText.innerHTML = storyStates.and;
        } else if (choice === 'not') {
            storyText.innerHTML = storyStates.not;
        }

        orButton.disabled = true;
        andButton.disabled = true;
        notButton.disabled = true;
    } else if (gameStarted && choice === initialChoice) {
        currentStep += 1;
        storyText.innerHTML = storyStates[`${currentChoice}${currentStep}`] || storyStates.end;

        if (!storyStates[`${currentChoice}${currentStep + 1}`]) {
            storyText.innerHTML += "<br><br>" + storyStates.end2 + "<br>" + storyStates.end3 + "<br>" + storyStates.end4 + "<br>" + storyStates.end5;
        }
    }
}

