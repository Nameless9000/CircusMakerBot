namespace Math

open System.Text.RegularExpressions

module Calculator =
    type BinOp = Add | Sub | Mul | Div | Rem | Exp | Xor | Or | And

    type Expr =
        | Number of float
        | BinaryOp of BinOp * Expr * Expr

    let rec eval = function
        | Number n -> n
        | BinaryOp (op, e1, e2) ->
            let f1 = eval e1 in
            let f2 = eval e2 in
            let i1 = int (System.Math.Round(f1)) in
            let i2 = int (System.Math.Round(f2)) in

            match op with
            | Add -> f1 + f2
            | Sub -> f1 - f2
            | Mul -> f1 * f2
            | Div -> f1 / f2
            | Rem -> f1 % f2
            | Exp -> f1 ** f2
            | Xor -> int (i1 ^^^ i2)
            | Or -> int (i1 ||| i2)
            | And -> int (i1 &&& i2)

    let rec parse_expr (tokens: list<string>) =
        let rec inner (acc: Expr) (tokens: list<string>) =
            match tokens with
            | [] -> acc
            | num :: _ when System.Double.TryParse(num).Equals(true) ->
                Number(float num)
            | symbol :: tail ->
                let op =
                    match symbol with
                    | "+" -> Add
                    | "-" -> Sub
                    | "*" -> Mul
                    | "/" -> Div
                    | "%" -> Rem
                    | "**" -> Exp
                    | "^" -> Xor
                    | "|" -> Or
                    | "&" -> And
                    | _ -> failwith "Unknown operator"

                BinaryOp(op, acc, parse_expr tail)

        if tokens.Length = 0 then
            failwith "Unexpected end of input"

        inner (Number(float (List.head tokens))) (List.tail tokens)

    let dump_var x = sprintf "%A" x

    let tokenize (input: string) =
        let matches = Regex.Matches(input, "((?<=^|[+\-*/^|&]\*?)[+-]?([0-9]*[.])?[0-9]+)|([+\-*/^|&]\*?)")
        [ for m in matches -> m.Value ]